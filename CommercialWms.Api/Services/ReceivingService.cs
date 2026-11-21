using System.Data;
using System.Globalization;
using CommercialWms.Api.Contracts.Receiving;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Services;

internal sealed class ReceivingOperationException(int statusCode, string title, string detail) : Exception(detail)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title;
}

public sealed class ReceivingService(CommercialWmsDbContext dbContext) : IReceivingService
{
    private const string Draft = "Draft";
    private const string Submitted = "Submitted";
    private const string Approved = "Approved";
    private const string PartiallyReceived = "PartiallyReceived";
    private const string Received = "Received";
    private const string PartiallyAccepted = "PartiallyAccepted";
    private const string Accepted = "Accepted";
    private const string Rejected = "Rejected";
    private const string Checking = "Checking";
    private const string Cancelled = "Cancelled";

    public async Task<IReadOnlyList<PurchaseOrderResponse>> GetPurchaseOrdersAsync(
        string? status,
        CancellationToken cancellationToken)
    {
        var query = dbContext.PurchaseOrders.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim();
            if (!IsPurchaseOrderStatus(normalizedStatus))
            {
                throw InvalidInput("The purchase-order status is not supported.");
            }

            query = query.Where(order => order.Status == normalizedStatus);
        }

        return await BuildPurchaseOrderResponsesAsync(query, cancellationToken);
    }

    public async Task<PurchaseOrderResponse> GetPurchaseOrderAsync(long id, CancellationToken cancellationToken)
    {
        var order = await dbContext.PurchaseOrders.AsNoTracking()
            .Where(item => item.PurchaseOrderId == id)
            .Select(item => new { item.PurchaseOrderId })
            .SingleOrDefaultAsync(cancellationToken);
        if (order is null)
        {
            throw NotFound("The purchase order was not found.");
        }

        var responses = await BuildPurchaseOrderResponsesAsync(
            dbContext.PurchaseOrders.AsNoTracking().Where(item => item.PurchaseOrderId == id),
            cancellationToken);
        return responses.Single();
    }

    public async Task<PurchaseOrderResponse> CreatePurchaseOrderAsync(
        CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        ValidatePurchaseOrderRequest(request);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        if (!await dbContext.Suppliers.AnyAsync(
                supplier => supplier.SupplierId == request.SupplierId && supplier.IsActive,
                cancellationToken))
        {
            throw InvalidReference("Select an active supplier.");
        }

        if (!await dbContext.Currencies.AnyAsync(
                currency => currency.CurrencyId == request.CurrencyId && currency.IsActive,
                cancellationToken))
        {
            throw InvalidReference("Select an active currency.");
        }

        if (request.CreatedByEmployeeId.HasValue
            && !await dbContext.Employees.AnyAsync(
                employee => employee.EmployeeId == request.CreatedByEmployeeId && employee.IsActive,
                cancellationToken))
        {
            throw InvalidReference("Select an active employee.");
        }

        var productIds = request.Lines.Select(line => line.ProductId).Distinct().ToArray();
        var unitPairs = request.Lines.Select(line => new { line.ProductId, line.UnitId }).ToArray();
        var validProductIds = await dbContext.Products
            .Where(product => productIds.Contains(product.ProductId) && product.IsActive)
            .Select(product => product.ProductId)
            .ToListAsync(cancellationToken);
        if (validProductIds.Count != productIds.Length)
        {
            throw InvalidReference("Every ordered product must exist and be active.");
        }

        var validUnitPairs = await (
            from link in dbContext.ProductUnits
            join unit in dbContext.Units on link.UnitId equals unit.UnitId
            where productIds.Contains(link.ProductId) && unit.IsActive
            select new { link.ProductId, link.UnitId, link.ConversionToBase })
            .ToListAsync(cancellationToken);
        if (unitPairs.Any(pair => !validUnitPairs.Any(
                valid => valid.ProductId == pair.ProductId
                    && valid.UnitId == pair.UnitId
                    && valid.ConversionToBase > 0)))
        {
            throw InvalidReference("Each product must use an active unit configured for that product.");
        }

        var supplierProducts = await dbContext.SupplierProducts
            .Where(item => item.SupplierId == request.SupplierId
                && productIds.Contains(item.ProductId)
                && item.IsActive)
            .Select(item => item.ProductId)
            .Distinct()
            .ToListAsync(cancellationToken);
        if (supplierProducts.Count != productIds.Length)
        {
            throw InvalidReference("Every ordered product must be actively supplied by the selected supplier.");
        }

        var order = new PurchaseOrder
        {
            PONumber = GenerateNumber("PO"),
            SupplierId = request.SupplierId,
            OrderDate = request.OrderDate.UtcDateTime,
            ExpectedDate = request.ExpectedDate,
            CurrencyId = request.CurrencyId,
            ExchangeRate = request.ExchangeRate,
            Status = Draft,
            CreatedByEmployeeId = request.CreatedByEmployeeId,
            Note = NormalizeOptional(request.Note)
        };

        dbContext.PurchaseOrders.Add(order);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            foreach (var line in request.Lines)
            {
                dbContext.PurchaseOrderDetails.Add(new PurchaseOrderDetail
                {
                    PurchaseOrderId = order.PurchaseOrderId,
                    ProductId = line.ProductId,
                    OrderedQuantity = line.OrderedQuantity,
                    UnitId = line.UnitId,
                    UnitPrice = line.UnitPrice
                });
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsSqlError(exception, 2601, 2627))
        {
            throw Conflict("A unique purchase-order value already exists. Retry the request.");
        }
        catch (DbUpdateException exception) when (IsSqlError(exception, 547))
        {
            throw InvalidReference("A supplier, currency, employee, product, or unit reference is no longer valid.");
        }

        return await GetPurchaseOrderAsync(order.PurchaseOrderId, cancellationToken);
    }

    public Task<PurchaseOrderResponse> SubmitPurchaseOrderAsync(long id, CancellationToken cancellationToken)
    {
        return ChangePurchaseOrderStatusAsync(id, Draft, Submitted, cancellationToken);
    }

    public async Task<PurchaseOrderResponse> ApprovePurchaseOrderAsync(long id, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var order = await dbContext.PurchaseOrders
            .SingleOrDefaultAsync(item => item.PurchaseOrderId == id, cancellationToken)
            ?? throw NotFound("The purchase order was not found.");
        if (order.Status != Submitted)
        {
            throw Conflict("Only a submitted purchase order can be approved.");
        }

        var lines = await dbContext.PurchaseOrderDetails
            .Where(line => line.PurchaseOrderId == id)
            .ToListAsync(cancellationToken);
        if (lines.Count == 0)
        {
            throw Conflict("A purchase order without lines cannot be approved.");
        }

        foreach (var line in lines)
        {
            dbContext.PurchasePriceHistories.Add(new PurchasePriceHistory
            {
                ProductId = line.ProductId,
                SupplierId = order.SupplierId,
                PurchaseOrderDetailId = line.PurchaseOrderDetailId,
                CurrencyId = order.CurrencyId,
                UnitPrice = line.UnitPrice
            });
        }

        order.Status = Approved;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsSqlError(exception, 547))
        {
            throw InvalidReference("A purchase-order reference is no longer valid.");
        }

        return await GetPurchaseOrderAsync(id, cancellationToken);
    }

    public async Task<PurchaseOrderResponse> CancelPurchaseOrderAsync(long id, CancellationToken cancellationToken)
    {
        var order = await dbContext.PurchaseOrders
            .SingleOrDefaultAsync(item => item.PurchaseOrderId == id, cancellationToken)
            ?? throw NotFound("The purchase order was not found.");
        if (order.Status is not (Draft or Submitted))
        {
            throw Conflict("Only draft or submitted purchase orders can be cancelled.");
        }

        if (await dbContext.GoodsReceipts.AnyAsync(receipt => receipt.PurchaseOrderId == id, cancellationToken))
        {
            throw Conflict("A purchase order with a goods receipt cannot be cancelled.");
        }

        order.Status = Cancelled;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetPurchaseOrderAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductUnitOptionResponse>> GetProductUnitsAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Products.AnyAsync(product => product.ProductId == productId, cancellationToken))
        {
            throw NotFound("The product was not found.");
        }

        return await (
            from link in dbContext.ProductUnits.AsNoTracking()
            join unit in dbContext.Units.AsNoTracking() on link.UnitId equals unit.UnitId
            where link.ProductId == productId && unit.IsActive
            orderby link.IsBaseUnit descending, unit.UnitName
            select new ProductUnitOptionResponse(
                unit.UnitId,
                unit.UnitCode,
                unit.UnitName,
                link.ConversionToBase,
                link.IsBaseUnit))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReceivingBinOptionResponse>> GetWarehouseBinsAsync(
        int warehouseId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Warehouses.AnyAsync(
                warehouse => warehouse.WarehouseId == warehouseId && warehouse.IsActive,
                cancellationToken))
        {
            throw NotFound("The active warehouse was not found.");
        }

        return await (
            from bin in dbContext.WarehouseBins.AsNoTracking()
            join shelf in dbContext.WarehouseShelves.AsNoTracking() on bin.ShelfId equals shelf.ShelfId
            join rack in dbContext.WarehouseRacks.AsNoTracking() on shelf.RackId equals rack.RackId
            join aisle in dbContext.WarehouseAisles.AsNoTracking() on rack.AisleId equals aisle.AisleId
            join zone in dbContext.WarehouseZones.AsNoTracking() on aisle.ZoneId equals zone.ZoneId
            join warehouse in dbContext.Warehouses.AsNoTracking() on zone.WarehouseId equals warehouse.WarehouseId
            where warehouse.WarehouseId == warehouseId && warehouse.IsActive && bin.IsActive
            orderby zone.ZoneCode, aisle.AisleCode, rack.RackCode, shelf.ShelfCode, bin.BinCode
            select new ReceivingBinOptionResponse(
                bin.BinId,
                bin.BinCode,
                shelf.ShelfCode,
                rack.RackCode,
                aisle.AisleCode,
                zone.ZoneCode,
                warehouse.WarehouseCode))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GoodsReceiptListItemResponse>> GetGoodsReceiptsAsync(
        string? search,
        string? status,
        int? supplierId,
        int? warehouseId,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
        {
            throw InvalidInput("The from date must not be later than the to date.");
        }

        if (supplierId is <= 0 || warehouseId is <= 0)
        {
            throw InvalidInput("Supplier and warehouse filters must be positive identifiers.");
        }

        var query = from receipt in dbContext.GoodsReceipts.AsNoTracking()
                    join order in dbContext.PurchaseOrders.AsNoTracking()
                        on receipt.PurchaseOrderId equals order.PurchaseOrderId
                    join supplier in dbContext.Suppliers.AsNoTracking()
                        on order.SupplierId equals supplier.SupplierId
                    join warehouse in dbContext.Warehouses.AsNoTracking()
                        on receipt.WarehouseId equals warehouse.WarehouseId
                    select new { receipt, order, supplier, warehouse };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(row => row.receipt.ReceiptNumber.Contains(value)
                || row.order.PONumber.Contains(value)
                || row.supplier.SupplierName.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim();
            if (!IsGoodsReceiptStatus(normalizedStatus))
            {
                throw InvalidInput("The goods-receipt status is not supported.");
            }

            query = query.Where(row => row.receipt.Status == normalizedStatus);
        }

        if (supplierId.HasValue)
        {
            query = query.Where(row => row.order.SupplierId == supplierId.Value);
        }

        if (warehouseId.HasValue)
        {
            query = query.Where(row => row.receipt.WarehouseId == warehouseId.Value);
        }

        if (fromDate.HasValue)
        {
            var start = fromDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(row => row.receipt.ReceivedAt >= start);
        }

        if (toDate.HasValue && toDate.Value < DateOnly.MaxValue)
        {
            var endExclusive = toDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            query = query.Where(row => row.receipt.ReceivedAt < endExclusive);
        }

        var rows = await query
            .OrderByDescending(row => row.receipt.ReceivedAt)
            .ThenByDescending(row => row.receipt.GoodsReceiptId)
            .Select(row => new
            {
                row.receipt.GoodsReceiptId,
                row.receipt.ReceiptNumber,
                row.receipt.PurchaseOrderId,
                row.order.PONumber,
                row.supplier.SupplierName,
                row.receipt.WarehouseId,
                row.warehouse.WarehouseName,
                row.receipt.ReceivedAt,
                row.receipt.Status
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return [];
        }

        var receiptIds = rows.Select(row => row.GoodsReceiptId).ToArray();
        var totals = await dbContext.GoodsReceiptDetails.AsNoTracking()
            .Where(line => receiptIds.Contains(line.GoodsReceiptId))
            .GroupBy(line => line.GoodsReceiptId)
            .Select(group => new
            {
                GoodsReceiptId = group.Key,
                LineCount = group.Count(),
                Received = group.Sum(line => line.ReceivedQuantity),
                Accepted = group.Sum(line => line.AcceptedQuantity),
                Rejected = group.Sum(line => line.RejectedQuantity)
            })
            .ToDictionaryAsync(item => item.GoodsReceiptId, cancellationToken);

        return rows.Select(row =>
        {
            totals.TryGetValue(row.GoodsReceiptId, out var total);
            return new GoodsReceiptListItemResponse(
                row.GoodsReceiptId,
                row.ReceiptNumber,
                row.PurchaseOrderId,
                row.PONumber,
                row.SupplierName,
                row.WarehouseId,
                row.WarehouseName,
                ToUtcOffset(row.ReceivedAt),
                row.Status,
                total?.LineCount ?? 0,
                total?.Received ?? 0,
                total?.Accepted ?? 0,
                total?.Rejected ?? 0);
        }).ToArray();
    }

    public Task<GoodsReceiptResponse> GetGoodsReceiptAsync(long id, CancellationToken cancellationToken)
    {
        return BuildGoodsReceiptResponseAsync(id, cancellationToken);
    }

    public async Task<GoodsReceiptResponse> CreateGoodsReceiptAsync(
        CreateGoodsReceiptRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var order = await dbContext.PurchaseOrders
            .SingleOrDefaultAsync(item => item.PurchaseOrderId == request.PurchaseOrderId, cancellationToken)
            ?? throw NotFound("The purchase order was not found.");
        if (order.Status is not (Approved or PartiallyReceived))
        {
            throw Conflict("Only approved or partially received purchase orders can be received.");
        }

        if (!await dbContext.Warehouses.AnyAsync(
                warehouse => warehouse.WarehouseId == request.WarehouseId && warehouse.IsActive,
                cancellationToken))
        {
            throw InvalidReference("Select an active warehouse.");
        }

        if (request.ReceivedByEmployeeId.HasValue
            && !await dbContext.Employees.AnyAsync(
                employee => employee.EmployeeId == request.ReceivedByEmployeeId && employee.IsActive,
                cancellationToken))
        {
            throw InvalidReference("Select an active receiving employee.");
        }

        var orderLines = await dbContext.PurchaseOrderDetails
            .Where(line => line.PurchaseOrderId == order.PurchaseOrderId)
            .OrderBy(line => line.PurchaseOrderDetailId)
            .ToListAsync(cancellationToken);
        if (orderLines.Count == 0)
        {
            throw Conflict("A purchase order without detail lines cannot be received.");
        }

        var previouslyAccepted = await GetAcceptedByOrderLineAsync(order.PurchaseOrderId, null, cancellationToken);
        if (orderLines.All(line =>
                previouslyAccepted.GetValueOrDefault(line.PurchaseOrderDetailId) >= line.OrderedQuantity))
        {
            throw Conflict("All purchase-order quantities have already been accepted.");
        }

        var receipt = new GoodsReceipt
        {
            ReceiptNumber = GenerateNumber("GR"),
            PurchaseOrderId = order.PurchaseOrderId,
            WarehouseId = request.WarehouseId,
            ReceivedAt = DateTime.UtcNow,
            ReceivedByEmployeeId = request.ReceivedByEmployeeId,
            Status = Draft,
            Note = NormalizeOptional(request.Note)
        };
        dbContext.GoodsReceipts.Add(receipt);
        await dbContext.SaveChangesAsync(cancellationToken);
        foreach (var line in orderLines)
        {
            dbContext.GoodsReceiptDetails.Add(new GoodsReceiptDetail
            {
                GoodsReceiptId = receipt.GoodsReceiptId,
                PurchaseOrderDetailId = line.PurchaseOrderDetailId,
                ProductId = line.ProductId,
                UnitId = line.UnitId,
                ReceivedQuantity = 0,
                AcceptedQuantity = 0,
                RejectedQuantity = 0
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await BuildGoodsReceiptResponseAsync(receipt.GoodsReceiptId, cancellationToken);
    }

    public async Task<GoodsReceiptResponse> SaveCheckInAsync(
        long id,
        GoodsReceiptCheckInRequest request,
        CancellationToken cancellationToken)
    {
        var receipt = await dbContext.GoodsReceipts
            .SingleOrDefaultAsync(item => item.GoodsReceiptId == id, cancellationToken)
            ?? throw NotFound("The goods receipt was not found.");
        if (receipt.Status is not (Draft or Checking))
        {
            throw Conflict("Only draft or checking receipts can be checked in.");
        }

        if (await dbContext.QualityInspections.AnyAsync(
                inspection => inspection.GoodsReceiptId == id,
                cancellationToken))
        {
            throw Conflict("Check-in quantities cannot change after quality inspection has started.");
        }

        var lines = await dbContext.GoodsReceiptDetails
            .Where(line => line.GoodsReceiptId == id)
            .ToListAsync(cancellationToken);
        if (request.Lines.GroupBy(line => line.GoodsReceiptDetailId).Any(group => group.Count() > 1))
        {
            throw InvalidInput("Provide each receipt line exactly once.");
        }

        var submittedLines = request.Lines.ToDictionary(line => line.GoodsReceiptDetailId);
        if (submittedLines.Count != lines.Count
            || lines.Any(line => !submittedLines.ContainsKey(line.GoodsReceiptDetailId)))
        {
            throw InvalidInput("Provide each receipt line exactly once.");
        }

        foreach (var line in lines)
        {
            var input = submittedLines[line.GoodsReceiptDetailId];
            if (!HasScale(input.ReceivedQuantity, 3)
                || !HasScale(input.AcceptedQuantity, 3)
                || !HasScale(input.RejectedQuantity, 3))
            {
                throw InvalidInput("Receipt quantities support at most three decimal places.");
            }

            if (input.AcceptedQuantity + input.RejectedQuantity > input.ReceivedQuantity)
            {
                throw InvalidInput("Accepted plus rejected quantity cannot exceed the received quantity.");
            }

            line.ReceivedQuantity = input.ReceivedQuantity;
            line.AcceptedQuantity = input.AcceptedQuantity;
            line.RejectedQuantity = input.RejectedQuantity;
        }

        receipt.Status = lines.Any(line => line.ReceivedQuantity > 0) ? Checking : Draft;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await BuildGoodsReceiptResponseAsync(id, cancellationToken);
    }

    public async Task<GoodsReceiptResponse> SaveQualityInspectionAsync(
        long id,
        SaveQualityInspectionRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var receipt = await dbContext.GoodsReceipts
            .SingleOrDefaultAsync(item => item.GoodsReceiptId == id, cancellationToken)
            ?? throw NotFound("The goods receipt was not found.");
        if (receipt.Status != Checking)
        {
            throw Conflict("Quality inspection is available only while a receipt is being checked.");
        }

        if (request.InspectorEmployeeId.HasValue
            && !await dbContext.Employees.AnyAsync(
                employee => employee.EmployeeId == request.InspectorEmployeeId && employee.IsActive,
                cancellationToken))
        {
            throw InvalidReference("Select an active inspector.");
        }

        var receiptLines = await dbContext.GoodsReceiptDetails
            .Where(line => line.GoodsReceiptId == id)
            .Select(line => new
            {
                line.GoodsReceiptDetailId,
                line.ReceivedQuantity,
                line.AcceptedQuantity,
                line.RejectedQuantity
            })
            .ToListAsync(cancellationToken);
        var lineIds = receiptLines.Select(line => line.GoodsReceiptDetailId).ToHashSet();
        var checks = request.Checks
            .Select(check => check with
            {
                CheckName = check.CheckName.Trim(),
                Note = NormalizeOptional(check.Note),
                Result = check.Result.Trim()
            })
            .ToArray();

        if (checks.Any(check => !lineIds.Contains(check.GoodsReceiptDetailId)))
        {
            throw InvalidInput("Every quality check must reference a detail line in this receipt.");
        }

        if (checks.GroupBy(check => (
                check.GoodsReceiptDetailId,
                CheckName: check.CheckName.ToUpperInvariant()))
            .Any(group => group.Count() > 1))
        {
            throw InvalidInput("A quality check name can appear only once per receipt line.");
        }

        if (receiptLines.Count == 0
            || receiptLines.Any(line => line.ReceivedQuantity <= 0
                || line.AcceptedQuantity + line.RejectedQuantity != line.ReceivedQuantity))
        {
            throw Conflict("Complete check-in quantities and classify every received unit before starting quality inspection.");
        }

        var receivedLineIds = receiptLines
            .Where(line => line.ReceivedQuantity > 0)
            .Select(line => line.GoodsReceiptDetailId)
            .ToHashSet();
        if (receivedLineIds.Any(lineId => !checks.Any(check => check.GoodsReceiptDetailId == lineId)))
        {
            throw InvalidInput("Every line with received quantity must have at least one quality check.");
        }

        var result = GetInspectionResult(checks);
        var inspection = await dbContext.QualityInspections
            .SingleOrDefaultAsync(item => item.GoodsReceiptId == id, cancellationToken);
        if (inspection is null)
        {
            inspection = new QualityInspection
            {
                InspectionNumber = GenerateNumber("QI"),
                GoodsReceiptId = id,
                InspectedAt = DateTime.UtcNow
            };
            dbContext.QualityInspections.Add(inspection);
        }
        else
        {
            var existingChecks = await dbContext.QualityInspectionDetails
                .Where(check => check.QualityInspectionId == inspection.QualityInspectionId)
                .ToListAsync(cancellationToken);
            dbContext.QualityInspectionDetails.RemoveRange(existingChecks);
        }

        inspection.InspectedAt = DateTime.UtcNow;
        inspection.InspectorEmployeeId = request.InspectorEmployeeId;
        inspection.Result = result;
        inspection.Note = NormalizeOptional(request.Note);

        await dbContext.SaveChangesAsync(cancellationToken);
        foreach (var check in checks)
        {
            dbContext.QualityInspectionDetails.Add(new QualityInspectionDetail
            {
                QualityInspectionId = inspection.QualityInspectionId,
                GoodsReceiptDetailId = check.GoodsReceiptDetailId,
                CheckName = check.CheckName,
                Result = check.Result,
                Note = check.Note
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await BuildGoodsReceiptResponseAsync(id, cancellationToken);
    }

    public async Task<GoodsReceiptResponse> ConfirmGoodsReceiptAsync(
        long id,
        ConfirmGoodsReceiptRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var receipt = await dbContext.GoodsReceipts
            .SingleOrDefaultAsync(item => item.GoodsReceiptId == id, cancellationToken)
            ?? throw NotFound("The goods receipt was not found.");
        if (receipt.Status != Checking)
        {
            throw Conflict("Only a receipt in checking status can be finalized.");
        }

        var order = await dbContext.PurchaseOrders
            .SingleAsync(item => item.PurchaseOrderId == receipt.PurchaseOrderId, cancellationToken);
        var inspection = await dbContext.QualityInspections
            .SingleOrDefaultAsync(item => item.GoodsReceiptId == id, cancellationToken)
            ?? throw Conflict("A quality inspection is required before finalizing a receipt.");
        if (inspection.Result is not ("Passed" or "Failed"))
        {
            throw Conflict("Inventory can be finalized only after a passed or failed quality inspection. Conditional and pending inspections require a separate business decision.");
        }

        var receiptLines = await dbContext.GoodsReceiptDetails
            .Where(line => line.GoodsReceiptId == id)
            .ToListAsync(cancellationToken);
        if (receiptLines.Count == 0 || !receiptLines.Any(line => line.ReceivedQuantity > 0))
        {
            throw Conflict("A receipt must contain a physically received quantity before it can be finalized.");
        }

        if (receiptLines.Any(line => line.AcceptedQuantity + line.RejectedQuantity != line.ReceivedQuantity))
        {
            throw Conflict("Every physically received unit must be classified as accepted or rejected before finalization.");
        }

        var orderedQuantities = await dbContext.PurchaseOrderDetails.AsNoTracking()
            .Where(line => line.PurchaseOrderId == order.PurchaseOrderId)
            .ToDictionaryAsync(line => line.PurchaseOrderDetailId, line => line.OrderedQuantity, cancellationToken);
        var previouslyAccepted = await GetAcceptedByOrderLineAsync(
            order.PurchaseOrderId,
            id,
            cancellationToken);
        if (receiptLines.Any(line =>
                !orderedQuantities.TryGetValue(line.PurchaseOrderDetailId, out var orderedQuantity)
                || previouslyAccepted.GetValueOrDefault(line.PurchaseOrderDetailId) + line.AcceptedQuantity
                    > orderedQuantity))
        {
            throw InvalidInput("Accepted quantities across receipts cannot exceed the purchase-order quantity.");
        }

        var detailIds = receiptLines.Select(line => line.GoodsReceiptDetailId).ToArray();
        if (await dbContext.InventoryTransactions.AnyAsync(
                item => item.ReferenceType == "GoodsReceiptDetail"
                    && item.ReferenceId.HasValue
                    && detailIds.Contains(item.ReferenceId.Value),
                cancellationToken))
        {
            throw Conflict("Inventory transactions already exist for this receipt. Duplicate stock posting was prevented.");
        }

        var acceptedTotal = receiptLines.Sum(line => line.AcceptedQuantity);
        var rejectedTotal = receiptLines.Sum(line => line.RejectedQuantity);
        var putawayItems = request.Putaway;

        if (inspection.Result == "Failed" && acceptedTotal > 0)
        {
            throw Conflict("A failed quality inspection cannot post accepted quantity to inventory.");
        }

        if (acceptedTotal == 0)
        {
            if (putawayItems.Count != 0)
            {
                throw InvalidInput("A receipt with no accepted quantity cannot contain put-away allocations.");
            }

            receipt.Status = Rejected;
            await dbContext.SaveChangesAsync(cancellationToken);
            await UpdatePurchaseOrderReceivingStatusAsync(order, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return await BuildGoodsReceiptResponseAsync(id, cancellationToken);
        }

        if (inspection.Result != "Passed")
        {
            throw Conflict("A passed quality inspection is required before accepted goods can enter inventory.");
        }

        var detailsById = receiptLines.ToDictionary(line => line.GoodsReceiptDetailId);
        var putawayByDetail = putawayItems.GroupBy(item => item.GoodsReceiptDetailId)
            .ToDictionary(group => group.Key, group => group.ToArray());
        if (putawayByDetail.Keys.Any(detailId => !detailsById.ContainsKey(detailId)))
        {
            throw InvalidInput("Every put-away item must reference a detail line in this receipt.");
        }

        foreach (var line in receiptLines)
        {
            var items = putawayByDetail.GetValueOrDefault(line.GoodsReceiptDetailId) ?? [];
            var allocated = items.Sum(item => item.Quantity);
            if (!HasScale(allocated, 3) || allocated != line.AcceptedQuantity)
            {
                throw InvalidInput("Put-away quantities must equal the accepted quantity for every receipt line.");
            }
        }

        var normalizedItems = putawayItems.Select(item => item with
        {
            LotCode = item.LotCode.Trim(),
            SupplierLotCode = NormalizeOptional(item.SupplierLotCode)
        }).ToArray();
        if (normalizedItems.Any(item => item.Quantity <= 0
                || !HasScale(item.Quantity, 3)
                || item.LotCode.Length == 0
                || item.ManufactureDate.HasValue
                    && item.ExpiryDate.HasValue
                    && item.ExpiryDate.Value < item.ManufactureDate.Value))
        {
            throw InvalidInput("Put-away quantities, lot codes, and lot dates are invalid.");
        }

        if (normalizedItems.GroupBy(item => (detailsById[item.GoodsReceiptDetailId].ProductId, NormalizeLotKey(item.LotCode)))
            .Any(group => group.Select(GetLotMetadataKey).Distinct().Count() > 1))
        {
            throw InvalidInput("The same product lot must use identical manufacturer, origin, and date values.");
        }

        var binIds = normalizedItems.Select(item => item.BinId).Distinct().ToArray();
        var productIds = normalizedItems.Select(item => detailsById[item.GoodsReceiptDetailId].ProductId).Distinct().ToArray();
        var manufacturerIds = normalizedItems.Select(item => item.ManufacturerId).Distinct().ToArray();
        var countryIds = normalizedItems.Select(item => item.CountryId).Distinct().ToArray();
        var lotCodes = normalizedItems.Select(item => item.LotCode).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        var validBins = await (
            from bin in dbContext.WarehouseBins
            join shelf in dbContext.WarehouseShelves on bin.ShelfId equals shelf.ShelfId
            join rack in dbContext.WarehouseRacks on shelf.RackId equals rack.RackId
            join aisle in dbContext.WarehouseAisles on rack.AisleId equals aisle.AisleId
            join zone in dbContext.WarehouseZones on aisle.ZoneId equals zone.ZoneId
            where binIds.Contains(bin.BinId) && bin.IsActive
            select new { bin.BinId, zone.WarehouseId })
            .ToDictionaryAsync(item => item.BinId, item => item.WarehouseId, cancellationToken);
        if (binIds.Any(binId => !validBins.TryGetValue(binId, out var ownerWarehouseId)
                || ownerWarehouseId != receipt.WarehouseId))
        {
            throw InvalidReference("Every put-away bin must be active and belong to the receipt warehouse.");
        }

        if (await dbContext.Manufacturers.CountAsync(
                item => manufacturerIds.Contains(item.ManufacturerId) && item.IsActive,
                cancellationToken) != manufacturerIds.Length
            || await dbContext.Countries.CountAsync(
                item => countryIds.Contains(item.CountryId) && item.IsActive,
                cancellationToken) != countryIds.Length)
        {
            throw InvalidReference("Every lot manufacturer and country of origin must be active.");
        }

        var supplierProductKeys = await dbContext.SupplierProducts
            .Where(item => item.SupplierId == order.SupplierId
                && productIds.Contains(item.ProductId)
                && manufacturerIds.Contains(item.ManufacturerId)
                && item.IsActive)
            .Select(item => new { item.ProductId, item.ManufacturerId })
            .ToListAsync(cancellationToken);
        var validSupplierProductKeys = supplierProductKeys
            .Select(item => (item.ProductId, item.ManufacturerId))
            .ToHashSet();
        if (normalizedItems.Any(item =>
                !validSupplierProductKeys.Contains((
                    detailsById[item.GoodsReceiptDetailId].ProductId,
                    item.ManufacturerId))))
        {
            throw InvalidReference("The lot manufacturer must be an active manufacturer for the supplier and product.");
        }

        var unitPairs = receiptLines.Select(line => (line.ProductId, line.UnitId)).Distinct().ToArray();
        var conversionRows = await (
            from link in dbContext.ProductUnits
            where productIds.Contains(link.ProductId)
            select new { link.ProductId, link.UnitId, link.ConversionToBase })
            .ToListAsync(cancellationToken);
        var conversions = conversionRows.ToDictionary(
            row => (row.ProductId, row.UnitId),
            row => row.ConversionToBase);
        if (unitPairs.Any(pair => !conversions.ContainsKey(pair) || conversions[pair] <= 0))
        {
            throw Conflict("A receipt product unit no longer has a valid conversion to its inventory base unit.");
        }

        var normalizedByLine = normalizedItems.GroupBy(item => item.GoodsReceiptDetailId);
        foreach (var group in normalizedByLine)
        {
            var line = detailsById[group.Key];
            var conversion = conversions[(line.ProductId, line.UnitId)];
            var expectedBaseQuantity = ToInventoryPrecision(line.AcceptedQuantity * conversion);
            var allocatedBaseQuantity = group.Sum(item => ToInventoryPrecision(item.Quantity * conversion));
            if (expectedBaseQuantity != allocatedBaseQuantity)
            {
                throw InvalidInput("The put-away split cannot be represented at the inventory quantity precision of three decimal places.");
            }
        }

        var lotMetadataByKey = new Dictionary<(int ProductId, string LotCode), GoodsReceiptPutawayItemRequest>();
        foreach (var item in normalizedItems)
        {
            var productId = detailsById[item.GoodsReceiptDetailId].ProductId;
            lotMetadataByKey.TryAdd((productId, NormalizeLotKey(item.LotCode)), item);
        }

        var existingLots = await dbContext.InventoryLots
            .Where(lot => productIds.Contains(lot.ProductId) && lotCodes.Contains(lot.LotCode))
            .ToListAsync(cancellationToken);
        var lotsByKey = existingLots.ToDictionary(
            lot => (lot.ProductId, NormalizeLotKey(lot.LotCode)));
        foreach (var entry in lotMetadataByKey)
        {
            var metadata = entry.Value;
            if (lotsByKey.TryGetValue(entry.Key, out var existingLot))
            {
                if (!LotMatches(existingLot, metadata))
                {
                    throw Conflict("The lot code already exists for this product with different lot details.");
                }

                continue;
            }

            var lot = new InventoryLot
            {
                ProductId = entry.Key.ProductId,
                LotCode = metadata.LotCode,
                ManufacturerId = metadata.ManufacturerId,
                CountryId = metadata.CountryId,
                ManufactureDate = metadata.ManufactureDate,
                ExpiryDate = metadata.ExpiryDate,
                SupplierLotCode = metadata.SupplierLotCode
            };
            dbContext.InventoryLots.Add(lot);
            lotsByKey.Add(entry.Key, lot);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var allocations = normalizedItems.Select(item =>
        {
            var detail = detailsById[item.GoodsReceiptDetailId];
            var lot = lotsByKey[(detail.ProductId, NormalizeLotKey(item.LotCode))];
            var conversion = conversions[(detail.ProductId, detail.UnitId)];
            return new
            {
                Item = item,
                Detail = detail,
                Lot = lot,
                BaseQuantity = ToInventoryPrecision(item.Quantity * conversion)
            };
        }).ToArray();

        var lotIds = allocations.Select(item => item.Lot.LotId).Distinct().ToArray();
        var existingBalances = await dbContext.InventoryBalances
            .Where(balance => lotIds.Contains(balance.LotId) && binIds.Contains(balance.BinId))
            .ToListAsync(cancellationToken);
        var balancesByKey = existingBalances.ToDictionary(balance => (balance.LotId, balance.BinId));
        var now = DateTime.UtcNow;
        foreach (var allocation in allocations)
        {
            var key = (allocation.Lot.LotId, allocation.Item.BinId);
            if (!balancesByKey.TryGetValue(key, out var balance))
            {
                balance = new InventoryBalance
                {
                    LotId = key.LotId,
                    BinId = key.BinId,
                    QuantityOnHand = 0,
                    QuantityReserved = 0,
                    UpdatedAt = now
                };
                dbContext.InventoryBalances.Add(balance);
                balancesByKey.Add(key, balance);
            }

            balance.QuantityOnHand += allocation.BaseQuantity;
            balance.UpdatedAt = now;
            dbContext.InventoryTransactions.Add(new InventoryTransaction
            {
                LotId = allocation.Lot.LotId,
                WarehouseId = receipt.WarehouseId,
                BinId = allocation.Item.BinId,
                TransactionType = "Receipt",
                QuantityChange = allocation.BaseQuantity,
                ReferenceType = "GoodsReceiptDetail",
                ReferenceId = allocation.Detail.GoodsReceiptDetailId,
                OccurredAt = now,
                CreatedByEmployeeId = receipt.ReceivedByEmployeeId,
                Note = $"Goods receipt {receipt.ReceiptNumber}"
            });
        }

        receipt.Status = acceptedTotal == receiptLines.Sum(line => line.ReceivedQuantity)
            ? Accepted
            : PartiallyAccepted;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await UpdatePurchaseOrderReceivingStatusAsync(order, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsSqlError(exception, 2601, 2627))
        {
            throw Conflict("A lot or inventory balance with the same unique key already exists. No receipt stock was committed.");
        }
        catch (DbUpdateException exception) when (IsSqlError(exception, 547))
        {
            throw InvalidReference("A warehouse, bin, lot, employee, or product reference is no longer valid.");
        }

        return await BuildGoodsReceiptResponseAsync(id, cancellationToken);
    }

    public async Task<GoodsReceiptResponse> CancelGoodsReceiptAsync(long id, CancellationToken cancellationToken)
    {
        var receipt = await dbContext.GoodsReceipts
            .SingleOrDefaultAsync(item => item.GoodsReceiptId == id, cancellationToken)
            ?? throw NotFound("The goods receipt was not found.");
        if (receipt.Status is not (Draft or Checking))
        {
            throw Conflict("Only draft or checking receipts can be cancelled.");
        }

        var detailIds = await dbContext.GoodsReceiptDetails
            .Where(line => line.GoodsReceiptId == id)
            .Select(line => line.GoodsReceiptDetailId)
            .ToArrayAsync(cancellationToken);
        if (await dbContext.InventoryTransactions.AnyAsync(
                item => item.ReferenceType == "GoodsReceiptDetail"
                    && item.ReferenceId.HasValue
                    && detailIds.Contains(item.ReferenceId.Value),
                cancellationToken))
        {
            throw Conflict("A receipt with posted stock transactions cannot be cancelled.");
        }

        receipt.Status = Cancelled;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await BuildGoodsReceiptResponseAsync(id, cancellationToken);
    }

    private async Task<PurchaseOrderResponse> ChangePurchaseOrderStatusAsync(
        long id,
        string expectedStatus,
        string nextStatus,
        CancellationToken cancellationToken)
    {
        var order = await dbContext.PurchaseOrders
            .SingleOrDefaultAsync(item => item.PurchaseOrderId == id, cancellationToken)
            ?? throw NotFound("The purchase order was not found.");
        if (order.Status != expectedStatus)
        {
            throw Conflict($"A purchase order must be {expectedStatus} before it can become {nextStatus}.");
        }

        order.Status = nextStatus;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetPurchaseOrderAsync(id, cancellationToken);
    }

    private async Task<IReadOnlyList<PurchaseOrderResponse>> BuildPurchaseOrderResponsesAsync(
        IQueryable<PurchaseOrder> ordersQuery,
        CancellationToken cancellationToken)
    {
        var headers = await (
            from order in ordersQuery
            join supplier in dbContext.Suppliers.AsNoTracking() on order.SupplierId equals supplier.SupplierId
            join currency in dbContext.Currencies.AsNoTracking() on order.CurrencyId equals currency.CurrencyId
            orderby order.OrderDate descending, order.PurchaseOrderId descending
            select new
            {
                order.PurchaseOrderId,
                order.PONumber,
                order.SupplierId,
                supplier.SupplierCode,
                supplier.SupplierName,
                order.CurrencyId,
                currency.CurrencyCode,
                currency.CurrencyName,
                order.OrderDate,
                order.ExpectedDate,
                order.ExchangeRate,
                order.Status,
                order.CreatedByEmployeeId,
                order.Note
            })
            .ToListAsync(cancellationToken);
        if (headers.Count == 0)
        {
            return [];
        }

        var orderIds = headers.Select(order => order.PurchaseOrderId).ToArray();
        var lineRows = await (
            from line in dbContext.PurchaseOrderDetails.AsNoTracking()
            join product in dbContext.Products.AsNoTracking() on line.ProductId equals product.ProductId
            join unit in dbContext.Units.AsNoTracking() on line.UnitId equals unit.UnitId
            where orderIds.Contains(line.PurchaseOrderId)
            orderby line.PurchaseOrderDetailId
            select new
            {
                line.PurchaseOrderId,
                line.PurchaseOrderDetailId,
                line.ProductId,
                product.SKU,
                product.ProductName,
                line.UnitId,
                unit.UnitCode,
                unit.UnitName,
                line.OrderedQuantity,
                line.UnitPrice
            })
            .ToListAsync(cancellationToken);
        var acceptedByLine = await GetAcceptedByOrderLineAsync(orderIds, null, cancellationToken);
        var linesByOrder = lineRows
            .GroupBy(line => line.PurchaseOrderId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<PurchaseOrderLineResponse>)group.Select(line =>
                    new PurchaseOrderLineResponse(
                        line.PurchaseOrderDetailId,
                        line.ProductId,
                        line.SKU,
                        line.ProductName,
                        line.UnitId,
                        line.UnitCode,
                        line.UnitName,
                        line.OrderedQuantity,
                        acceptedByLine.GetValueOrDefault(line.PurchaseOrderDetailId),
                        line.UnitPrice)).ToArray());

        return headers.Select(order => new PurchaseOrderResponse(
            order.PurchaseOrderId,
            order.PONumber,
            order.SupplierId,
            order.SupplierCode,
            order.SupplierName,
            order.CurrencyId,
            order.CurrencyCode,
            order.CurrencyName,
            ToUtcOffset(order.OrderDate),
            order.ExpectedDate,
            order.ExchangeRate,
            order.Status,
            order.CreatedByEmployeeId,
            order.Note,
            linesByOrder.GetValueOrDefault(order.PurchaseOrderId) ?? []))
            .ToArray();
    }

    private async Task<GoodsReceiptResponse> BuildGoodsReceiptResponseAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var header = await (
            from receipt in dbContext.GoodsReceipts.AsNoTracking()
            join order in dbContext.PurchaseOrders.AsNoTracking()
                on receipt.PurchaseOrderId equals order.PurchaseOrderId
            join currency in dbContext.Currencies.AsNoTracking()
                on order.CurrencyId equals currency.CurrencyId
            join supplier in dbContext.Suppliers.AsNoTracking()
                on order.SupplierId equals supplier.SupplierId
            join warehouse in dbContext.Warehouses.AsNoTracking()
                on receipt.WarehouseId equals warehouse.WarehouseId
            where receipt.GoodsReceiptId == id
            select new
            {
                receipt.GoodsReceiptId,
                receipt.ReceiptNumber,
                receipt.PurchaseOrderId,
                order.PONumber,
                currency.CurrencyCode,
                order.SupplierId,
                supplier.SupplierName,
                receipt.WarehouseId,
                warehouse.WarehouseCode,
                warehouse.WarehouseName,
                receipt.ReceivedAt,
                receipt.ReceivedByEmployeeId,
                receipt.Status,
                receipt.Note
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw NotFound("The goods receipt was not found.");

        var lineRows = await (
            from receiptLine in dbContext.GoodsReceiptDetails.AsNoTracking()
            join orderLine in dbContext.PurchaseOrderDetails.AsNoTracking()
                on receiptLine.PurchaseOrderDetailId equals orderLine.PurchaseOrderDetailId
            join product in dbContext.Products.AsNoTracking() on receiptLine.ProductId equals product.ProductId
            join unit in dbContext.Units.AsNoTracking() on receiptLine.UnitId equals unit.UnitId
            where receiptLine.GoodsReceiptId == id
            orderby receiptLine.GoodsReceiptDetailId
            select new
            {
                receiptLine.GoodsReceiptDetailId,
                receiptLine.PurchaseOrderDetailId,
                receiptLine.ProductId,
                product.SKU,
                product.ProductName,
                receiptLine.UnitId,
                unit.UnitCode,
                unit.UnitName,
                orderLine.OrderedQuantity,
                orderLine.UnitPrice,
                receiptLine.ReceivedQuantity,
                receiptLine.AcceptedQuantity,
                receiptLine.RejectedQuantity
            })
            .ToListAsync(cancellationToken);
        var acceptedByOrderLine = await GetAcceptedByOrderLineAsync(
            header.PurchaseOrderId,
            id,
            cancellationToken);
        var lines = lineRows.Select(line => new GoodsReceiptDetailResponse(
            line.GoodsReceiptDetailId,
            line.PurchaseOrderDetailId,
            line.ProductId,
            line.SKU,
            line.ProductName,
            line.UnitId,
            line.UnitCode,
            line.UnitName,
            line.OrderedQuantity,
            acceptedByOrderLine.GetValueOrDefault(line.PurchaseOrderDetailId),
            line.UnitPrice,
            line.ReceivedQuantity,
            line.AcceptedQuantity,
            line.RejectedQuantity))
            .ToArray();

        QualityInspectionResponse? inspectionResponse = null;
        var inspection = await dbContext.QualityInspections.AsNoTracking()
            .Where(item => item.GoodsReceiptId == id)
            .Select(item => new
            {
                item.QualityInspectionId,
                item.InspectionNumber,
                item.InspectedAt,
                item.InspectorEmployeeId,
                item.Result,
                item.Note
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (inspection is not null)
        {
            var checks = await dbContext.QualityInspectionDetails.AsNoTracking()
                .Where(check => check.QualityInspectionId == inspection.QualityInspectionId)
                .OrderBy(check => check.QualityInspectionDetailId)
                .Select(check => new QualityInspectionCheckResponse(
                    check.QualityInspectionDetailId,
                    check.GoodsReceiptDetailId,
                    check.CheckName,
                    check.Result,
                    check.Note))
                .ToListAsync(cancellationToken);
            inspectionResponse = new QualityInspectionResponse(
                inspection.QualityInspectionId,
                inspection.InspectionNumber,
                ToUtcOffset(inspection.InspectedAt),
                inspection.InspectorEmployeeId,
                inspection.Result,
                inspection.Note,
                checks);
        }

        return new GoodsReceiptResponse(
            header.GoodsReceiptId,
            header.ReceiptNumber,
            header.PurchaseOrderId,
            header.PONumber,
            header.CurrencyCode,
            header.SupplierId,
            header.SupplierName,
            header.WarehouseId,
            header.WarehouseCode,
            header.WarehouseName,
            ToUtcOffset(header.ReceivedAt),
            header.ReceivedByEmployeeId,
            header.Status,
            header.Note,
            lines,
            inspectionResponse);
    }

    private async Task<Dictionary<long, decimal>> GetAcceptedByOrderLineAsync(
        long purchaseOrderId,
        long? excludeReceiptId,
        CancellationToken cancellationToken)
    {
        var query = from receiptLine in dbContext.GoodsReceiptDetails.AsNoTracking()
                    join receipt in dbContext.GoodsReceipts.AsNoTracking()
                        on receiptLine.GoodsReceiptId equals receipt.GoodsReceiptId
                    where receipt.PurchaseOrderId == purchaseOrderId
                        && (receipt.Status == Accepted || receipt.Status == PartiallyAccepted)
                    select new { receiptLine, receipt };
        if (excludeReceiptId.HasValue)
        {
            query = query.Where(row => row.receipt.GoodsReceiptId != excludeReceiptId.Value);
        }

        return await query
            .GroupBy(row => row.receiptLine.PurchaseOrderDetailId)
            .Select(group => new
            {
                PurchaseOrderDetailId = group.Key,
                Quantity = group.Sum(row => row.receiptLine.AcceptedQuantity)
            })
            .ToDictionaryAsync(row => row.PurchaseOrderDetailId, row => row.Quantity, cancellationToken);
    }

    private async Task<Dictionary<long, decimal>> GetAcceptedByOrderLineAsync(
        IReadOnlyCollection<long> purchaseOrderIds,
        long? excludeReceiptId,
        CancellationToken cancellationToken)
    {
        var query = from receiptLine in dbContext.GoodsReceiptDetails.AsNoTracking()
                    join receipt in dbContext.GoodsReceipts.AsNoTracking()
                        on receiptLine.GoodsReceiptId equals receipt.GoodsReceiptId
                    where purchaseOrderIds.Contains(receipt.PurchaseOrderId)
                        && (receipt.Status == Accepted || receipt.Status == PartiallyAccepted)
                    select new { receiptLine, receipt };
        if (excludeReceiptId.HasValue)
        {
            query = query.Where(row => row.receipt.GoodsReceiptId != excludeReceiptId.Value);
        }

        return await query
            .GroupBy(row => row.receiptLine.PurchaseOrderDetailId)
            .Select(group => new
            {
                PurchaseOrderDetailId = group.Key,
                Quantity = group.Sum(row => row.receiptLine.AcceptedQuantity)
            })
            .ToDictionaryAsync(row => row.PurchaseOrderDetailId, row => row.Quantity, cancellationToken);
    }

    private async Task UpdatePurchaseOrderReceivingStatusAsync(
        PurchaseOrder order,
        CancellationToken cancellationToken)
    {
        var lines = await dbContext.PurchaseOrderDetails.AsNoTracking()
            .Where(line => line.PurchaseOrderId == order.PurchaseOrderId)
            .Select(line => new { line.PurchaseOrderDetailId, line.OrderedQuantity })
            .ToListAsync(cancellationToken);
        var accepted = await GetAcceptedByOrderLineAsync(
            order.PurchaseOrderId,
            null,
            cancellationToken);
        if (lines.Count == 0)
        {
            throw Conflict("A purchase order without lines cannot receive stock.");
        }

        order.Status = lines.All(line =>
                accepted.GetValueOrDefault(line.PurchaseOrderDetailId) >= line.OrderedQuantity)
            ? Received
            : lines.Any(line => accepted.GetValueOrDefault(line.PurchaseOrderDetailId) > 0)
                ? PartiallyReceived
                : Approved;
    }

    private static void ValidatePurchaseOrderRequest(CreatePurchaseOrderRequest request)
    {
        if (request.Lines.Count == 0)
        {
            throw InvalidInput("A purchase order must include at least one product line.");
        }

        if (request.OrderDate == default)
        {
            throw InvalidInput("An order date is required.");
        }

        var orderDate = request.OrderDate.UtcDateTime;
        if (request.ExpectedDate.HasValue
            && request.ExpectedDate.Value < DateOnly.FromDateTime(orderDate))
        {
            throw InvalidInput("The expected delivery date cannot be earlier than the order date.");
        }

        if (request.ExchangeRate <= 0 || !HasScale(request.ExchangeRate, 6))
        {
            throw InvalidInput("Exchange rate must be positive and have at most six decimal places.");
        }

        if (request.Lines.Any(line => line.OrderedQuantity <= 0
                || !HasScale(line.OrderedQuantity, 3)
                || line.UnitPrice < 0
                || !HasScale(line.UnitPrice, 4)))
        {
            throw InvalidInput("Order quantities must be positive with at most three decimals, and unit prices may have at most four decimals.");
        }

        if (request.Lines.GroupBy(line => (line.ProductId, line.UnitId)).Any(group => group.Count() > 1))
        {
            throw InvalidInput("A product and unit combination can appear only once in a purchase order.");
        }
    }

    private static string GetInspectionResult(IReadOnlyCollection<SaveQualityInspectionCheckRequest> checks)
    {
        if (checks.Count == 0)
        {
            throw InvalidInput("At least one quality check is required.");
        }

        if (checks.Any(check => check.Result == "Fail"))
        {
            return "Failed";
        }

        if (checks.Any(check => check.Result == "Pending"))
        {
            return "Pending";
        }

        return checks.All(check => check.Result == "NotApplicable")
            ? "Conditional"
            : "Passed";
    }

    private static string GenerateNumber(string prefix)
    {
        return string.Concat(
            prefix,
            "-",
            DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture),
            "-",
            Guid.NewGuid().ToString("N")[..8].ToUpperInvariant());
    }

    private static DateTimeOffset ToUtcOffset(DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
        return new DateTimeOffset(utc);
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }

    private static bool HasScale(decimal value, int scale)
    {
        return value == decimal.Round(value, scale);
    }

    private static decimal ToInventoryPrecision(decimal value)
    {
        return decimal.Round(value, 3, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeLotKey(string lotCode)
    {
        return lotCode.Trim().ToUpperInvariant();
    }

    private static (int ManufacturerId, int CountryId, DateOnly? ManufactureDate, DateOnly? ExpiryDate, string? SupplierLotCode)
        GetLotMetadataKey(GoodsReceiptPutawayItemRequest item)
    {
        return (
            item.ManufacturerId,
            item.CountryId,
            item.ManufactureDate,
            item.ExpiryDate,
            item.SupplierLotCode?.ToUpperInvariant());
    }

    private static bool LotMatches(InventoryLot lot, GoodsReceiptPutawayItemRequest item)
    {
        return lot.ManufacturerId == item.ManufacturerId
            && lot.CountryId == item.CountryId
            && lot.ManufactureDate == item.ManufactureDate
            && lot.ExpiryDate == item.ExpiryDate
            && string.Equals(lot.SupplierLotCode, item.SupplierLotCode, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPurchaseOrderStatus(string status)
    {
        return status is Draft or Submitted or Approved or PartiallyReceived or Received or Cancelled;
    }

    private static bool IsGoodsReceiptStatus(string status)
    {
        return status is Draft or Checking or Accepted or PartiallyAccepted or Rejected or Cancelled;
    }

    private static bool IsSqlError(DbUpdateException exception, params int[] numbers)
    {
        return exception.GetBaseException() is SqlException sqlException
            && numbers.Contains(sqlException.Number);
    }

    private static ReceivingOperationException InvalidInput(string detail)
    {
        return new ReceivingOperationException(StatusCodes.Status400BadRequest, "Invalid receiving input", detail);
    }

    private static ReceivingOperationException InvalidReference(string detail)
    {
        return new ReceivingOperationException(StatusCodes.Status400BadRequest, "Invalid receiving reference", detail);
    }

    private static ReceivingOperationException NotFound(string detail)
    {
        return new ReceivingOperationException(StatusCodes.Status404NotFound, "Resource not found", detail);
    }

    private static ReceivingOperationException Conflict(string detail)
    {
        return new ReceivingOperationException(StatusCodes.Status409Conflict, "Receiving conflict", detail);
    }
}
