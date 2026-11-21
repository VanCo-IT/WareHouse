using System.Data;
using System.Globalization;
using CommercialWms.Api.Contracts.Outbound;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Services;

internal sealed class OutboundOperationException(int statusCode, string title, string detail) : Exception(detail)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title;
}

public sealed class OutboundService(CommercialWmsDbContext dbContext) : IOutboundService
{
    private const string Draft = "Draft";
    private const string Confirmed = "Confirmed";
    private const string PartiallyDelivered = "PartiallyDelivered";
    private const string Delivered = "Delivered";
    private const string Cancelled = "Cancelled";
    private const string ActiveReservation = "Active";
    private const string ReleasedReservation = "Released";
    private const string FulfilledReservation = "Fulfilled";
    private const string DeliveryDraft = "Draft";
    private const string Picking = "Picking";
    private const string Shipped = "Shipped";
    private const string DeliveryDelivered = "Delivered";
    private const string DeliveryCancelled = "Cancelled";
    private const decimal MaximumQuantity = 999_999_999_999_999.999m;
    private const decimal MaximumAmount = 99_999_999_999_999.9999m;
    private const decimal MaximumExchangeRate = 999_999_999_999.999999m;
    private const int MaximumPageSize = 100;

    public async Task<OutboundOptionsResponse> GetOptionsAsync(CancellationToken cancellationToken)
    {
        var addresses = await dbContext.CustomerAddresses.AsNoTracking()
            .OrderByDescending(item => item.IsDefault)
            .ThenBy(item => item.AddressId)
            .Select(item => new OutboundAddressOptionResponse(
                item.AddressId,
                item.AddressLine,
                item.Province,
                item.District,
                item.IsDefault))
            .ToListAsync(cancellationToken);
        var addressOwners = await dbContext.CustomerAddresses.AsNoTracking()
            .Select(item => new { item.CustomerId, item.AddressId })
            .ToListAsync(cancellationToken);
        var addressesByCustomer = addressOwners
            .Join(addresses, owner => owner.AddressId, address => address.AddressId,
                (owner, address) => new { owner.CustomerId, Address = address })
            .GroupBy(item => item.CustomerId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<OutboundAddressOptionResponse>)group
                .Select(item => item.Address)
                .ToArray());
        var customers = await dbContext.Customers.AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.CustomerName)
            .Select(item => new { item.CustomerId, item.CustomerCode, item.CustomerName })
            .ToListAsync(cancellationToken);
        var currencies = await dbContext.Currencies.AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.CurrencyCode)
            .Select(item => new OutboundCurrencyOptionResponse(
                item.CurrencyId,
                item.CurrencyCode,
                item.CurrencyName))
            .ToListAsync(cancellationToken);
        var productUnits = await (
            from link in dbContext.ProductUnits.AsNoTracking()
            join unit in dbContext.Units.AsNoTracking() on link.UnitId equals unit.UnitId
            where unit.IsActive
            orderby link.IsBaseUnit descending, unit.UnitName
            select new
            {
                link.ProductId,
                Unit = new OutboundUnitOptionResponse(
                    unit.UnitId,
                    unit.UnitCode,
                    unit.UnitName,
                    link.ConversionToBase,
                    link.IsBaseUnit)
            }).ToListAsync(cancellationToken);
        var unitsByProduct = productUnits
            .GroupBy(item => item.ProductId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<OutboundUnitOptionResponse>)group
                .Select(item => item.Unit)
                .ToArray());
        var products = await dbContext.Products.AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.ProductName)
            .Select(item => new { item.ProductId, item.SKU, item.ProductName })
            .ToListAsync(cancellationToken);
        var warehouses = await dbContext.Warehouses.AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.WarehouseName)
            .Select(item => new OutboundWarehouseOptionResponse(
                item.WarehouseId,
                item.WarehouseCode,
                item.WarehouseName))
            .ToListAsync(cancellationToken);
        var employees = await dbContext.Employees.AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.FullName)
            .Select(item => new OutboundEmployeeOptionResponse(
                item.EmployeeId,
                item.EmployeeCode,
                item.FullName))
            .ToListAsync(cancellationToken);

        return new OutboundOptionsResponse(
            customers.Select(item => new OutboundCustomerOptionResponse(
                item.CustomerId,
                item.CustomerCode,
                item.CustomerName,
                addressesByCustomer.GetValueOrDefault(item.CustomerId, []))).ToArray(),
            currencies,
            products.Where(item => unitsByProduct.ContainsKey(item.ProductId))
                .Select(item => new OutboundProductOptionResponse(
                    item.ProductId,
                    item.SKU,
                    item.ProductName,
                    unitsByProduct[item.ProductId]))
                .ToArray(),
            warehouses,
            employees);
    }

    public async Task<IReadOnlyList<OutboundAvailableStockResponse>> GetAvailableStockAsync(
        int warehouseId,
        int productId,
        int unitId,
        CancellationToken cancellationToken)
    {
        if (warehouseId <= 0 || productId <= 0 || unitId <= 0)
        {
            throw InvalidInput("Warehouse, product, and unit identifiers must be positive.");
        }

        var conversion = await (
            from link in dbContext.ProductUnits.AsNoTracking()
            join unit in dbContext.Units.AsNoTracking() on link.UnitId equals unit.UnitId
            where link.ProductId == productId && link.UnitId == unitId && unit.IsActive
            select (decimal?)link.ConversionToBase).SingleOrDefaultAsync(cancellationToken);
        if (!conversion.HasValue || conversion.Value <= 0)
        {
            throw InvalidReference("The selected unit is not configured for this product.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var rows = await (
            from balance in dbContext.InventoryBalances.AsNoTracking()
            join lot in dbContext.InventoryLots.AsNoTracking() on balance.LotId equals lot.LotId
            join bin in dbContext.WarehouseBins.AsNoTracking() on balance.BinId equals bin.BinId
            join shelf in dbContext.WarehouseShelves.AsNoTracking() on bin.ShelfId equals shelf.ShelfId
            join rack in dbContext.WarehouseRacks.AsNoTracking() on shelf.RackId equals rack.RackId
            join aisle in dbContext.WarehouseAisles.AsNoTracking() on rack.AisleId equals aisle.AisleId
            join zone in dbContext.WarehouseZones.AsNoTracking() on aisle.ZoneId equals zone.ZoneId
            join warehouse in dbContext.Warehouses.AsNoTracking() on zone.WarehouseId equals warehouse.WarehouseId
            where lot.ProductId == productId
                && warehouse.WarehouseId == warehouseId
                && warehouse.IsActive
                && bin.IsActive
                && balance.QuantityOnHand > balance.QuantityReserved
                && (lot.ExpiryDate ?? today) >= today
            orderby lot.ExpiryDate, lot.CreatedAt, zone.ZoneCode, aisle.AisleCode, rack.RackCode, shelf.ShelfCode, bin.BinCode
            select new
            {
                balance.LotId,
                lot.LotCode,
                lot.ExpiryDate,
                bin.BinId,
                bin.BinCode,
                LocationCode = zone.ZoneCode + "-" + aisle.AisleCode + "-" + rack.RackCode + "-" + shelf.ShelfCode,
                balance.QuantityOnHand,
                balance.QuantityReserved
            }).ToListAsync(cancellationToken);

        return rows.Select(row =>
        {
            var available = row.QuantityOnHand - row.QuantityReserved;
            return new OutboundAvailableStockResponse(
                row.LotId,
                row.LotCode,
                row.ExpiryDate,
                row.BinId,
                row.BinCode,
                row.LocationCode,
                row.QuantityOnHand,
                row.QuantityReserved,
                available,
                decimal.Round(available / conversion.Value, 3, MidpointRounding.ToZero));
        }).ToArray();
    }

    public async Task<OutboundPageResponse<OutboundOrderListItemResponse>> GetOrdersAsync(
        string? search,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ValidatePaging(page, pageSize);
        var query =
            from order in dbContext.SalesOrders.AsNoTracking()
            join customer in dbContext.Customers.AsNoTracking() on order.CustomerId equals customer.CustomerId
            join currency in dbContext.Currencies.AsNoTracking() on order.CurrencyId equals currency.CurrencyId
            select new { order, customer, currency };
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(row => row.order.SalesOrderNumber.Contains(value)
                || row.customer.CustomerCode.Contains(value)
                || row.customer.CustomerName.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var value = status.Trim();
            if (!IsOrderStatus(value))
            {
                throw InvalidInput("The sales-order status is not supported.");
            }

            query = query.Where(row => row.order.Status == value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(row => row.order.OrderDate)
            .ThenByDescending(row => row.order.SalesOrderId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(row => new OutboundOrderListItemResponse(
                row.order.SalesOrderId,
                row.order.SalesOrderNumber,
                row.customer.CustomerId,
                row.customer.CustomerCode,
                row.customer.CustomerName,
                row.order.OrderDate,
                row.currency.CurrencyId,
                row.currency.CurrencyCode,
                row.order.ExchangeRate,
                row.order.Status,
                dbContext.SalesOrderDetails.Count(line => line.SalesOrderId == row.order.SalesOrderId),
                dbContext.Deliveries.Count(delivery => delivery.SalesOrderId == row.order.SalesOrderId)))
            .ToListAsync(cancellationToken);

        return new OutboundPageResponse<OutboundOrderListItemResponse>(items, page, pageSize, totalCount);
    }

    public async Task<OutboundOrderResponse> GetOrderAsync(long id, CancellationToken cancellationToken)
    {
        var header = await (
            from order in dbContext.SalesOrders.AsNoTracking()
            join customer in dbContext.Customers.AsNoTracking() on order.CustomerId equals customer.CustomerId
            join currency in dbContext.Currencies.AsNoTracking() on order.CurrencyId equals currency.CurrencyId
            where order.SalesOrderId == id
            select new { order, customer, currency }).SingleOrDefaultAsync(cancellationToken)
            ?? throw NotFound("The sales order was not found.");

        var lines = await (
            from line in dbContext.SalesOrderDetails.AsNoTracking()
            join product in dbContext.Products.AsNoTracking() on line.ProductId equals product.ProductId
            join unit in dbContext.Units.AsNoTracking() on line.UnitId equals unit.UnitId
            join productUnit in dbContext.ProductUnits.AsNoTracking()
                on new { line.ProductId, line.UnitId } equals new { productUnit.ProductId, productUnit.UnitId }
            where line.SalesOrderId == id
            orderby line.SalesOrderDetailId
            select new
            {
                line.SalesOrderDetailId,
                line.ProductId,
                product.SKU,
                product.ProductName,
                line.Quantity,
                line.UnitId,
                unit.UnitCode,
                productUnit.ConversionToBase,
                line.UnitPrice,
                line.DiscountAmount
            }).ToListAsync(cancellationToken);
        var storedLineCount = await dbContext.SalesOrderDetails.CountAsync(
            item => item.SalesOrderId == id,
            cancellationToken);
        if (lines.Count != storedLineCount || lines.Any(item => item.ConversionToBase <= 0))
        {
            throw Conflict("A sales-order line is missing a valid product-unit conversion.");
        }

        var lineIds = lines.Select(item => item.SalesOrderDetailId).ToArray();
        var deliveredByLine = await GetDeliveredQuantitiesAsync(lineIds, cancellationToken);
        var reservationRows = await (
            from reservation in dbContext.StockReservations.AsNoTracking()
            join lot in dbContext.InventoryLots.AsNoTracking() on reservation.LotId equals lot.LotId
            join bin in dbContext.WarehouseBins.AsNoTracking() on reservation.BinId equals bin.BinId
            where reservation.SalesOrderDetailId.HasValue
                && lineIds.Contains(reservation.SalesOrderDetailId.Value)
                && reservation.Status == ActiveReservation
            orderby reservation.ReservationId
            select new
            {
                DetailId = reservation.SalesOrderDetailId!.Value,
                reservation.ReservationId,
                reservation.LotId,
                lot.LotCode,
                reservation.BinId,
                bin.BinCode,
                reservation.ReservedQuantity
            }).ToListAsync(cancellationToken);
        var reservationsByLine = reservationRows
            .GroupBy(item => item.DetailId)
            .ToDictionary(group => group.Key, group => group.ToArray());
        var responseLines = lines.Select(line => new OutboundOrderLineResponse(
            line.SalesOrderDetailId,
            line.ProductId,
            line.SKU,
            line.ProductName,
            line.Quantity,
            line.UnitId,
            line.UnitCode,
            line.ConversionToBase,
            line.UnitPrice,
            line.DiscountAmount,
            deliveredByLine.GetValueOrDefault(line.SalesOrderDetailId),
            reservationsByLine.GetValueOrDefault(line.SalesOrderDetailId, [])
                .Select(item => new OutboundReservationResponse(
                    item.ReservationId,
                    item.LotId,
                    item.LotCode,
                    item.BinId,
                    item.BinCode,
                    item.ReservedQuantity,
                    decimal.Round(item.ReservedQuantity / line.ConversionToBase, 3, MidpointRounding.ToZero)))
                .ToArray())).ToArray();
        var deliveries = await GetOrderDeliveriesAsync(id, cancellationToken);

        return new OutboundOrderResponse(
            header.order.SalesOrderId,
            header.order.SalesOrderNumber,
            header.customer.CustomerId,
            header.customer.CustomerCode,
            header.customer.CustomerName,
            header.order.OrderDate,
            header.currency.CurrencyId,
            header.currency.CurrencyCode,
            header.order.ExchangeRate,
            header.order.Status,
            header.order.CreatedByEmployeeId,
            header.order.Note,
            responseLines,
            deliveries);
    }

    public async Task<OutboundOrderResponse> CreateOrderAsync(
        CreateOutboundOrderRequest request,
        CancellationToken cancellationToken)
    {
        ValidateCreateOrder(request);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        if (!await dbContext.Customers.AnyAsync(
                item => item.CustomerId == request.CustomerId && item.IsActive,
                cancellationToken))
        {
            throw InvalidReference("Select an active customer.");
        }

        if (!await dbContext.Currencies.AnyAsync(
                item => item.CurrencyId == request.CurrencyId && item.IsActive,
                cancellationToken))
        {
            throw InvalidReference("Select an active currency.");
        }

        if (request.CreatedByEmployeeId.HasValue
            && !await dbContext.Employees.AnyAsync(
                item => item.EmployeeId == request.CreatedByEmployeeId && item.IsActive,
                cancellationToken))
        {
            throw InvalidReference("Select an active employee.");
        }

        var productIds = request.Lines.Select(item => item.ProductId).Distinct().ToArray();
        var activeProducts = await dbContext.Products
            .Where(item => productIds.Contains(item.ProductId) && item.IsActive)
            .Select(item => item.ProductId)
            .ToListAsync(cancellationToken);
        if (activeProducts.Count != productIds.Length)
        {
            throw InvalidReference("Every ordered product must exist and be active.");
        }

        var validUnits = await (
            from link in dbContext.ProductUnits
            join unit in dbContext.Units on link.UnitId equals unit.UnitId
            where productIds.Contains(link.ProductId) && unit.IsActive && link.ConversionToBase > 0
            select new { link.ProductId, link.UnitId }).ToListAsync(cancellationToken);
        if (request.Lines.Any(line => !validUnits.Any(
                item => item.ProductId == line.ProductId && item.UnitId == line.UnitId)))
        {
            throw InvalidReference("Each product must use an active unit configured for that product.");
        }

        var order = new SalesOrder
        {
            SalesOrderNumber = GenerateNumber("SO"),
            CustomerId = request.CustomerId,
            OrderDate = DateTime.UtcNow,
            CurrencyId = request.CurrencyId,
            ExchangeRate = request.ExchangeRate,
            Status = Draft,
            CreatedByEmployeeId = request.CreatedByEmployeeId,
            Note = NormalizeOptional(request.Note)
        };
        dbContext.SalesOrders.Add(order);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            foreach (var line in request.Lines)
            {
                dbContext.SalesOrderDetails.Add(new SalesOrderDetail
                {
                    SalesOrderId = order.SalesOrderId,
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    UnitId = line.UnitId,
                    UnitPrice = line.UnitPrice,
                    DiscountAmount = line.DiscountAmount
                });
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsSqlError(exception, 2601, 2627))
        {
            throw Conflict("A unique sales-order number already exists. Retry the request.");
        }
        catch (DbUpdateException exception) when (IsSqlError(exception, 547))
        {
            throw InvalidReference("A customer, currency, employee, product, or unit reference is no longer valid.");
        }

        return await GetOrderAsync(order.SalesOrderId, cancellationToken);
    }

    public async Task<OutboundOrderResponse> ConfirmOrderAsync(
        long id,
        ConfirmOutboundOrderRequest request,
        CancellationToken cancellationToken)
    {
        ValidateConfirmRequest(request);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var order = await dbContext.SalesOrders
            .SingleOrDefaultAsync(item => item.SalesOrderId == id, cancellationToken)
            ?? throw NotFound("The sales order was not found.");
        if (order.Status is not (Draft or PartiallyDelivered))
        {
            throw Conflict("Only a draft or partially delivered order can be prepared for another delivery.");
        }

        if (await dbContext.Deliveries.AnyAsync(
                item => item.SalesOrderId == id && (item.Status == DeliveryDraft || item.Status == Picking),
                cancellationToken))
        {
            throw Conflict("This order already has a delivery awaiting shipment.");
        }

        var warehouse = await dbContext.Warehouses
            .SingleOrDefaultAsync(item => item.WarehouseId == request.WarehouseId && item.IsActive, cancellationToken);
        if (warehouse is null)
        {
            throw InvalidReference("Select an active warehouse.");
        }

        if (request.DeliveryAddressId.HasValue
            && !await dbContext.CustomerAddresses.AnyAsync(
                item => item.AddressId == request.DeliveryAddressId && item.CustomerId == order.CustomerId,
                cancellationToken))
        {
            throw InvalidReference("The delivery address must belong to the selected customer.");
        }

        var lines = await (
            from line in dbContext.SalesOrderDetails
            join product in dbContext.Products on line.ProductId equals product.ProductId
            join productUnit in dbContext.ProductUnits
                on new { line.ProductId, line.UnitId } equals new { productUnit.ProductId, productUnit.UnitId }
            where line.SalesOrderId == id
            select new { Line = line, product.IsActive, productUnit.ConversionToBase })
            .ToListAsync(cancellationToken);
        var storedLineCount = await dbContext.SalesOrderDetails.CountAsync(
            item => item.SalesOrderId == id,
            cancellationToken);
        if (lines.Count == 0)
        {
            throw Conflict("A sales order without lines cannot be confirmed.");
        }

        if (lines.Count != storedLineCount || lines.Any(item => item.ConversionToBase <= 0))
        {
            throw Conflict("A sales-order line is missing a valid product-unit conversion.");
        }

        if (lines.Any(item => !item.IsActive))
        {
            throw Conflict("An inactive product cannot be prepared for shipment.");
        }

        var lineIds = lines.Select(item => item.Line.SalesOrderDetailId).ToArray();
        var deliveredByLine = await GetDeliveredQuantitiesAsync(lineIds, cancellationToken);
        var expectedByLine = lines.ToDictionary(
            item => item.Line.SalesOrderDetailId,
            item => item.Line.Quantity - deliveredByLine.GetValueOrDefault(item.Line.SalesOrderDetailId));
        if (expectedByLine.Values.Any(quantity => quantity < 0))
        {
            throw Conflict("Previously delivered quantities exceed the sales-order quantity.");
        }

        var lineById = lines.ToDictionary(item => item.Line.SalesOrderDetailId);
        var allocationTotals = new Dictionary<(long DetailId, long LotId, int BinId), decimal>();
        foreach (var allocation in request.Allocations)
        {
            if (!lineById.ContainsKey(allocation.SalesOrderDetailId))
            {
                throw InvalidReference("An allocation references a line outside this sales order.");
            }

            var key = (allocation.SalesOrderDetailId, allocation.LotId, allocation.BinId);
            allocationTotals[key] = allocationTotals.GetValueOrDefault(key) + allocation.Quantity;
        }

        foreach (var line in lines)
        {
            var lineId = line.Line.SalesOrderDetailId;
            var requestedQuantity = allocationTotals
                .Where(item => item.Key.DetailId == lineId)
                .Sum(item => item.Value);
            if (requestedQuantity != expectedByLine[lineId])
            {
                throw InvalidInput("Allocations for each order line must exactly match its undelivered quantity.");
            }
        }

        var binIds = allocationTotals.Keys.Select(key => key.BinId).Distinct().ToArray();
        var binWarehouses = await (
            from bin in dbContext.WarehouseBins
            join shelf in dbContext.WarehouseShelves on bin.ShelfId equals shelf.ShelfId
            join rack in dbContext.WarehouseRacks on shelf.RackId equals rack.RackId
            join aisle in dbContext.WarehouseAisles on rack.AisleId equals aisle.AisleId
            join zone in dbContext.WarehouseZones on aisle.ZoneId equals zone.ZoneId
            where binIds.Contains(bin.BinId) && bin.IsActive
            select new { bin.BinId, zone.WarehouseId }).ToListAsync(cancellationToken);
        var warehouseByBin = binWarehouses.ToDictionary(item => item.BinId, item => item.WarehouseId);
        if (allocationTotals.Keys.Any(key => !warehouseByBin.TryGetValue(key.BinId, out var warehouseId)
            || warehouseId != request.WarehouseId))
        {
            throw InvalidReference("Every selected bin must be active and belong to the delivery warehouse.");
        }

        var lotIds = allocationTotals.Keys.Select(key => key.LotId).Distinct().ToArray();
        var lots = await dbContext.InventoryLots
            .Where(item => lotIds.Contains(item.LotId))
            .ToDictionaryAsync(item => item.LotId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var allocation in allocationTotals)
        {
            var line = lineById[allocation.Key.DetailId];
            if (!lots.TryGetValue(allocation.Key.LotId, out var lot) || lot.ProductId != line.Line.ProductId)
            {
                throw InvalidReference("Each allocated lot must belong to the selected order-line product.");
            }

            if (lot.ExpiryDate.HasValue && lot.ExpiryDate.GetValueOrDefault() < today)
            {
                throw Conflict("Expired lots cannot be reserved for shipment.");
            }
        }

        var reservations = await dbContext.StockReservations
            .Where(item => item.SalesOrderDetailId.HasValue
                && lineIds.Contains(item.SalesOrderDetailId.Value)
                && item.Status == ActiveReservation)
            .OrderBy(item => item.ReservationId)
            .ToListAsync(cancellationToken);
        var balances = new Dictionary<(long LotId, int BinId), InventoryBalance>();
        var remainingBaseByAllocation = allocationTotals.ToDictionary(
            item => item.Key,
            item => ToBaseQuantity(item.Value, lineById[item.Key.DetailId].ConversionToBase));

        foreach (var reservation in reservations)
        {
            var key = (reservation.SalesOrderDetailId!.Value, reservation.LotId, reservation.BinId);
            var remaining = remainingBaseByAllocation.GetValueOrDefault(key);
            var kept = Math.Min(reservation.ReservedQuantity, remaining);
            var released = reservation.ReservedQuantity - kept;
            if (released > 0)
            {
                var balance = await GetBalanceAsync(reservation.LotId, reservation.BinId, balances, cancellationToken);
                if (balance.QuantityReserved < released)
                {
                    throw Conflict("The balance reservation is inconsistent with its active reservation record.");
                }

                balance.QuantityReserved -= released;
                balance.UpdatedAt = DateTime.UtcNow;
                reservation.ReservedQuantity = kept;
                if (kept == 0)
                {
                    reservation.Status = ReleasedReservation;
                }
            }

            remainingBaseByAllocation[key] = remaining - kept;
        }

        foreach (var allocation in remainingBaseByAllocation.Where(item => item.Value > 0))
        {
            var balance = await GetBalanceAsync(allocation.Key.LotId, allocation.Key.BinId, balances, cancellationToken);
            if (balance.QuantityOnHand - balance.QuantityReserved < allocation.Value)
            {
                throw Conflict("The requested allocation exceeds available stock in a selected lot and bin.");
            }

            balance.QuantityReserved += allocation.Value;
            balance.UpdatedAt = DateTime.UtcNow;
            dbContext.StockReservations.Add(new StockReservation
            {
                LotId = allocation.Key.LotId,
                BinId = allocation.Key.BinId,
                SalesOrderDetailId = allocation.Key.DetailId,
                ReservedQuantity = allocation.Value,
                Status = ActiveReservation,
                CreatedAt = DateTime.UtcNow
            });
        }

        var delivery = new Delivery
        {
            DeliveryNumber = GenerateNumber("DO"),
            SalesOrderId = order.SalesOrderId,
            WarehouseId = request.WarehouseId,
            DeliveryAddressId = request.DeliveryAddressId,
            PlannedAt = DateTime.UtcNow,
            Status = DeliveryDraft
        };
        dbContext.Deliveries.Add(delivery);
        order.Status = deliveredByLine.Values.Any(quantity => quantity > 0)
            ? PartiallyDelivered
            : Confirmed;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            foreach (var group in allocationTotals.GroupBy(item => (item.Key.DetailId, item.Key.LotId)))
            {
                dbContext.DeliveryDetails.Add(new DeliveryDetail
                {
                    DeliveryId = delivery.DeliveryId,
                    SalesOrderDetailId = group.Key.DetailId,
                    LotId = group.Key.LotId,
                    Quantity = group.Sum(item => item.Value)
                });
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsSqlError(exception, 2601, 2627))
        {
            throw Conflict("A unique delivery number already exists. Retry the request.");
        }
        catch (DbUpdateException exception) when (IsSqlError(exception, 547))
        {
            throw InvalidReference("A warehouse, address, lot, bin, or order-line reference is no longer valid.");
        }

        return await GetOrderAsync(id, cancellationToken);
    }

    public async Task<OutboundOrderResponse> CancelOrderAsync(long id, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var order = await dbContext.SalesOrders
            .SingleOrDefaultAsync(item => item.SalesOrderId == id, cancellationToken)
            ?? throw NotFound("The sales order was not found.");
        var lineIds = await dbContext.SalesOrderDetails
            .Where(item => item.SalesOrderId == id)
            .Select(item => item.SalesOrderDetailId)
            .ToArrayAsync(cancellationToken);
        if (order.Status != Draft
            || await dbContext.Deliveries.AnyAsync(
                item => item.SalesOrderId == id && item.Status != DeliveryCancelled,
                cancellationToken)
            || await dbContext.StockReservations.AnyAsync(
                item => item.SalesOrderDetailId.HasValue
                    && lineIds.Contains(item.SalesOrderDetailId.Value)
                    && item.Status == ActiveReservation,
                cancellationToken))
        {
            throw Conflict("Only a draft order without active deliveries or reservations can be cancelled.");
        }

        order.Status = Cancelled;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetOrderAsync(id, cancellationToken);
    }

    public async Task<OutboundPageResponse<OutboundDeliveryListItemResponse>> GetDeliveriesAsync(
        string? search,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ValidatePaging(page, pageSize);
        var query =
            from delivery in dbContext.Deliveries.AsNoTracking()
            join warehouse in dbContext.Warehouses.AsNoTracking() on delivery.WarehouseId equals warehouse.WarehouseId
            select new { delivery, warehouse };
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(row => row.delivery.DeliveryNumber.Contains(value)
                || dbContext.SalesOrders.Any(order => order.SalesOrderId == row.delivery.SalesOrderId
                    && order.SalesOrderNumber.Contains(value)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var value = status.Trim();
            if (!IsDeliveryStatus(value))
            {
                throw InvalidInput("The delivery status is not supported.");
            }

            query = query.Where(row => row.delivery.Status == value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(row => row.delivery.PlannedAt)
            .ThenByDescending(row => row.delivery.DeliveryId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(row => new OutboundDeliveryListItemResponse(
                row.delivery.DeliveryId,
                row.delivery.DeliveryNumber,
                row.warehouse.WarehouseId,
                row.warehouse.WarehouseCode,
                row.warehouse.WarehouseName,
                row.delivery.DeliveryAddressId,
                row.delivery.PlannedAt,
                row.delivery.ShippedAt,
                row.delivery.Status,
                dbContext.DeliveryDetails.Count(detail => detail.DeliveryId == row.delivery.DeliveryId)))
            .ToListAsync(cancellationToken);
        return new OutboundPageResponse<OutboundDeliveryListItemResponse>(items, page, pageSize, totalCount);
    }

    public async Task<OutboundDeliveryResponse> GetDeliveryAsync(long id, CancellationToken cancellationToken)
    {
        var header = await (
            from delivery in dbContext.Deliveries.AsNoTracking()
            join order in dbContext.SalesOrders.AsNoTracking() on delivery.SalesOrderId equals order.SalesOrderId
            join customer in dbContext.Customers.AsNoTracking() on order.CustomerId equals customer.CustomerId
            join warehouse in dbContext.Warehouses.AsNoTracking() on delivery.WarehouseId equals warehouse.WarehouseId
            where delivery.DeliveryId == id
            select new { delivery, order, customer, warehouse }).SingleOrDefaultAsync(cancellationToken)
            ?? throw NotFound("The delivery was not found.");
        var lines = await (
            from detail in dbContext.DeliveryDetails.AsNoTracking()
            join orderLine in dbContext.SalesOrderDetails.AsNoTracking()
                on detail.SalesOrderDetailId equals orderLine.SalesOrderDetailId
            join product in dbContext.Products.AsNoTracking() on orderLine.ProductId equals product.ProductId
            join unit in dbContext.Units.AsNoTracking() on orderLine.UnitId equals unit.UnitId
            join lot in dbContext.InventoryLots.AsNoTracking() on detail.LotId equals lot.LotId
            where detail.DeliveryId == id
                && orderLine.SalesOrderId == header.order.SalesOrderId
                && lot.ProductId == product.ProductId
            orderby detail.DeliveryDetailId
            select new OutboundDeliveryLineResponse(
                detail.DeliveryDetailId,
                detail.SalesOrderDetailId,
                product.ProductId,
                product.SKU,
                product.ProductName,
                lot.LotId,
                lot.LotCode,
                detail.Quantity,
                unit.UnitCode)).ToListAsync(cancellationToken);
        var storedDetailCount = await dbContext.DeliveryDetails.CountAsync(
            item => item.DeliveryId == id,
            cancellationToken);
        if (lines.Count != storedDetailCount)
        {
            throw Conflict("The delivery contains a line with an invalid order or lot relationship.");
        }

        return new OutboundDeliveryResponse(
            header.delivery.DeliveryId,
            header.delivery.DeliveryNumber,
            header.order.SalesOrderId,
            header.order.SalesOrderNumber,
            header.customer.CustomerName,
            header.warehouse.WarehouseId,
            header.warehouse.WarehouseCode,
            header.warehouse.WarehouseName,
            header.delivery.DeliveryAddressId,
            header.delivery.PlannedAt,
            header.delivery.ShippedAt,
            header.delivery.Status,
            lines);
    }

    public async Task<OutboundDeliveryResponse> PrepareDeliveryAsync(long id, CancellationToken cancellationToken)
    {
        var delivery = await dbContext.Deliveries
            .SingleOrDefaultAsync(item => item.DeliveryId == id, cancellationToken)
            ?? throw NotFound("The delivery was not found.");
        if (delivery.Status != DeliveryDraft)
        {
            throw Conflict("Only a draft delivery can enter picking.");
        }

        delivery.Status = Picking;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetDeliveryAsync(id, cancellationToken);
    }

    public async Task<OutboundDeliveryResponse> ShipDeliveryAsync(
        long id,
        ShipOutboundDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var delivery = await dbContext.Deliveries
            .SingleOrDefaultAsync(item => item.DeliveryId == id, cancellationToken)
            ?? throw NotFound("The delivery was not found.");
        if (delivery.Status != Picking)
        {
            throw Conflict("Only a delivery in picking status can be shipped.");
        }

        if (request.ShippedByEmployeeId.HasValue
            && !await dbContext.Employees.AnyAsync(
                item => item.EmployeeId == request.ShippedByEmployeeId && item.IsActive,
                cancellationToken))
        {
            throw InvalidReference("Select an active employee.");
        }

        var detailRows = await (
            from detail in dbContext.DeliveryDetails
            join line in dbContext.SalesOrderDetails on detail.SalesOrderDetailId equals line.SalesOrderDetailId
            join productUnit in dbContext.ProductUnits
                on new { line.ProductId, line.UnitId } equals new { productUnit.ProductId, productUnit.UnitId }
            where detail.DeliveryId == id
            select new { Detail = detail, line.SalesOrderId, line.ProductId, productUnit.ConversionToBase })
            .ToListAsync(cancellationToken);
        if (detailRows.Count == 0)
        {
            throw Conflict("A delivery without lines cannot be shipped.");
        }

        if (detailRows.Count != await dbContext.DeliveryDetails.CountAsync(
                item => item.DeliveryId == id,
                cancellationToken))
        {
            throw Conflict("A delivery line is missing a valid product-unit conversion.");
        }

        if (detailRows.Any(item => item.SalesOrderId != delivery.SalesOrderId))
        {
            throw Conflict("The delivery contains a line from a different sales order.");
        }

        var lineIds = detailRows.Select(item => item.Detail.SalesOrderDetailId).Distinct().ToArray();
        var activeReservations = await dbContext.StockReservations
            .Where(item => item.SalesOrderDetailId.HasValue
                && lineIds.Contains(item.SalesOrderDetailId.Value)
                && item.Status == ActiveReservation)
            .OrderBy(item => item.CreatedAt)
            .ThenBy(item => item.ReservationId)
            .ToListAsync(cancellationToken);
        var detailLotIds = detailRows.Select(row => row.Detail.LotId).Distinct().ToArray();
        var lotProductIds = await dbContext.InventoryLots
            .Where(item => detailLotIds.Contains(item.LotId))
            .ToDictionaryAsync(item => item.LotId, item => item.ProductId, cancellationToken);
        if (detailRows.Any(item => !lotProductIds.TryGetValue(item.Detail.LotId, out var productId)
            || productId != item.ProductId))
        {
            throw Conflict("The delivery contains a lot that does not match its order-line product.");
        }

        var selectedLotIds = detailRows.Select(item => item.Detail.LotId).Distinct().ToArray();
        var reservationBinIds = activeReservations
            .Where(item => selectedLotIds.Contains(item.LotId))
            .Select(item => item.BinId)
            .Distinct()
            .ToArray();
        var reservationWarehouses = await (
            from bin in dbContext.WarehouseBins
            join shelf in dbContext.WarehouseShelves on bin.ShelfId equals shelf.ShelfId
            join rack in dbContext.WarehouseRacks on shelf.RackId equals rack.RackId
            join aisle in dbContext.WarehouseAisles on rack.AisleId equals aisle.AisleId
            join zone in dbContext.WarehouseZones on aisle.ZoneId equals zone.ZoneId
            where reservationBinIds.Contains(bin.BinId) && bin.IsActive
            select new { bin.BinId, zone.WarehouseId }).ToListAsync(cancellationToken);
        var reservationWarehouseByBin = reservationWarehouses.ToDictionary(item => item.BinId, item => item.WarehouseId);
        if (activeReservations.Any(item => selectedLotIds.Contains(item.LotId)
            && (!reservationWarehouseByBin.TryGetValue(item.BinId, out var warehouseId)
                || warehouseId != delivery.WarehouseId)))
        {
            throw Conflict("An active delivery reservation is not in the delivery warehouse.");
        }

        var balances = new Dictionary<(long LotId, int BinId), InventoryBalance>();
        var issuePlans = new List<(StockReservation Reservation, InventoryBalance Balance, decimal Quantity)>();
        foreach (var detail in detailRows)
        {
            var remaining = ToBaseQuantity(detail.Detail.Quantity, detail.ConversionToBase);
            var candidates = activeReservations.Where(item =>
                item.SalesOrderDetailId == detail.Detail.SalesOrderDetailId
                && item.LotId == detail.Detail.LotId);
            foreach (var reservation in candidates)
            {
                if (remaining == 0)
                {
                    break;
                }

                var quantity = Math.Min(remaining, reservation.ReservedQuantity);
                var balance = await GetBalanceAsync(reservation.LotId, reservation.BinId, balances, cancellationToken);
                if (balance.QuantityReserved < quantity || balance.QuantityOnHand < quantity)
                {
                    throw Conflict("A reserved balance is insufficient to ship the delivery.");
                }

                issuePlans.Add((reservation, balance, quantity));
                remaining -= quantity;
            }

            if (remaining > 0)
            {
                throw Conflict("The delivery quantity is not fully covered by active reservations.");
            }
        }

        var now = DateTime.UtcNow;
        foreach (var plan in issuePlans)
        {
            plan.Balance.QuantityOnHand -= plan.Quantity;
            plan.Balance.QuantityReserved -= plan.Quantity;
            plan.Balance.UpdatedAt = now;
            plan.Reservation.ReservedQuantity -= plan.Quantity;
            if (plan.Reservation.ReservedQuantity == 0)
            {
                plan.Reservation.Status = FulfilledReservation;
            }

            dbContext.InventoryTransactions.Add(new InventoryTransaction
            {
                LotId = plan.Reservation.LotId,
                WarehouseId = delivery.WarehouseId,
                BinId = plan.Reservation.BinId,
                TransactionType = "Issue",
                QuantityChange = -plan.Quantity,
                ReferenceType = "Delivery",
                ReferenceId = delivery.DeliveryId,
                OccurredAt = now,
                CreatedByEmployeeId = request.ShippedByEmployeeId,
                Note = delivery.DeliveryNumber
            });
        }

        delivery.Status = Shipped;
        delivery.ShippedAt = now;
        var orderId = detailRows[0].SalesOrderId;
        var order = await dbContext.SalesOrders
            .SingleAsync(item => item.SalesOrderId == orderId, cancellationToken);
        var previousShipped = await GetDeliveredQuantitiesAsync(
            await dbContext.SalesOrderDetails
                .Where(item => item.SalesOrderId == orderId)
                .Select(item => item.SalesOrderDetailId)
                .ToArrayAsync(cancellationToken),
            cancellationToken);
        foreach (var detail in detailRows)
        {
            previousShipped[detail.Detail.SalesOrderDetailId] =
                previousShipped.GetValueOrDefault(detail.Detail.SalesOrderDetailId) + detail.Detail.Quantity;
        }

        var orderQuantities = await dbContext.SalesOrderDetails
            .Where(item => item.SalesOrderId == orderId)
            .Select(item => new { item.SalesOrderDetailId, item.Quantity })
            .ToListAsync(cancellationToken);
        order.Status = orderQuantities.All(item =>
                previousShipped.GetValueOrDefault(item.SalesOrderDetailId) >= item.Quantity)
            ? Delivered
            : PartiallyDelivered;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsSqlError(exception, 547))
        {
            throw InvalidReference("An inventory, employee, warehouse, or delivery reference is no longer valid.");
        }

        return await GetDeliveryAsync(id, cancellationToken);
    }

    public async Task<OutboundDeliveryResponse> MarkDeliveredAsync(long id, CancellationToken cancellationToken)
    {
        var delivery = await dbContext.Deliveries
            .SingleOrDefaultAsync(item => item.DeliveryId == id, cancellationToken)
            ?? throw NotFound("The delivery was not found.");
        if (delivery.Status != Shipped)
        {
            throw Conflict("Only a shipped delivery can be marked delivered.");
        }

        delivery.Status = DeliveryDelivered;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetDeliveryAsync(id, cancellationToken);
    }

    public async Task<OutboundDeliveryResponse> CancelDeliveryAsync(long id, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var delivery = await dbContext.Deliveries
            .SingleOrDefaultAsync(item => item.DeliveryId == id, cancellationToken)
            ?? throw NotFound("The delivery was not found.");
        if (delivery.Status is not (DeliveryDraft or Picking))
        {
            throw Conflict("Only an unshipped delivery can be cancelled.");
        }

        var details = await (
            from detail in dbContext.DeliveryDetails
            join line in dbContext.SalesOrderDetails on detail.SalesOrderDetailId equals line.SalesOrderDetailId
            join productUnit in dbContext.ProductUnits
                on new { line.ProductId, line.UnitId } equals new { productUnit.ProductId, productUnit.UnitId }
            where detail.DeliveryId == id
            select new { Detail = detail, line.SalesOrderId, productUnit.ConversionToBase })
            .ToListAsync(cancellationToken);
        if (details.Count != await dbContext.DeliveryDetails.CountAsync(
                item => item.DeliveryId == id,
                cancellationToken))
        {
            throw Conflict("A delivery line is missing a valid product-unit conversion.");
        }

        if (details.Any(item => item.SalesOrderId != delivery.SalesOrderId))
        {
            throw Conflict("The delivery contains a line from a different sales order.");
        }
        var lineIds = details.Select(item => item.Detail.SalesOrderDetailId).Distinct().ToArray();
        var reservations = await dbContext.StockReservations
            .Where(item => item.SalesOrderDetailId.HasValue
                && lineIds.Contains(item.SalesOrderDetailId.Value)
                && item.Status == ActiveReservation)
            .OrderBy(item => item.ReservationId)
            .ToListAsync(cancellationToken);
        var balances = new Dictionary<(long LotId, int BinId), InventoryBalance>();
        foreach (var detail in details)
        {
            var remaining = ToBaseQuantity(detail.Detail.Quantity, detail.ConversionToBase);
            foreach (var reservation in reservations.Where(item =>
                         item.SalesOrderDetailId == detail.Detail.SalesOrderDetailId
                         && item.LotId == detail.Detail.LotId))
            {
                if (remaining == 0)
                {
                    break;
                }

                var quantity = Math.Min(remaining, reservation.ReservedQuantity);
                var balance = await GetBalanceAsync(reservation.LotId, reservation.BinId, balances, cancellationToken);
                if (balance.QuantityReserved < quantity)
                {
                    throw Conflict("The reservation cannot be released because its balance is inconsistent.");
                }

                balance.QuantityReserved -= quantity;
                balance.UpdatedAt = DateTime.UtcNow;
                reservation.ReservedQuantity -= quantity;
                if (reservation.ReservedQuantity == 0)
                {
                    reservation.Status = ReleasedReservation;
                }

                remaining -= quantity;
            }

            if (remaining > 0)
            {
                throw Conflict("The delivery reservation is no longer available to release.");
            }
        }

        delivery.Status = DeliveryCancelled;
        var order = await dbContext.SalesOrders
            .SingleAsync(item => item.SalesOrderId == delivery.SalesOrderId, cancellationToken);
        var orderLineIds = await dbContext.SalesOrderDetails
            .Where(item => item.SalesOrderId == order.SalesOrderId)
            .Select(item => item.SalesOrderDetailId)
            .ToArrayAsync(cancellationToken);
        var shipped = await GetDeliveredQuantitiesAsync(orderLineIds, cancellationToken);
        order.Status = shipped.Values.Any(quantity => quantity > 0) ? PartiallyDelivered : Draft;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetDeliveryAsync(id, cancellationToken);
    }

    private async Task<IReadOnlyList<OutboundDeliveryListItemResponse>> GetOrderDeliveriesAsync(
        long orderId,
        CancellationToken cancellationToken)
    {
        return await (
            from delivery in dbContext.Deliveries.AsNoTracking()
            join warehouse in dbContext.Warehouses.AsNoTracking() on delivery.WarehouseId equals warehouse.WarehouseId
            where delivery.SalesOrderId == orderId
            orderby delivery.DeliveryId
            select new OutboundDeliveryListItemResponse(
                delivery.DeliveryId,
                delivery.DeliveryNumber,
                warehouse.WarehouseId,
                warehouse.WarehouseCode,
                warehouse.WarehouseName,
                delivery.DeliveryAddressId,
                delivery.PlannedAt,
                delivery.ShippedAt,
                delivery.Status,
                dbContext.DeliveryDetails.Count(detail => detail.DeliveryId == delivery.DeliveryId)))
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<long, decimal>> GetDeliveredQuantitiesAsync(
        IReadOnlyCollection<long> lineIds,
        CancellationToken cancellationToken)
    {
        if (lineIds.Count == 0)
        {
            return [];
        }

        var rows = await (
            from detail in dbContext.DeliveryDetails.AsNoTracking()
            join delivery in dbContext.Deliveries.AsNoTracking() on detail.DeliveryId equals delivery.DeliveryId
            where lineIds.Contains(detail.SalesOrderDetailId)
                && (delivery.Status == Shipped || delivery.Status == DeliveryDelivered)
            group detail by detail.SalesOrderDetailId into grouped
            select new { DetailId = grouped.Key, Quantity = grouped.Sum(item => item.Quantity) })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(item => item.DetailId, item => item.Quantity);
    }

    private async Task<InventoryBalance> GetBalanceAsync(
        long lotId,
        int binId,
        IDictionary<(long LotId, int BinId), InventoryBalance> cache,
        CancellationToken cancellationToken)
    {
        var key = (lotId, binId);
        if (cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var balance = await dbContext.InventoryBalances
            .SingleOrDefaultAsync(item => item.LotId == lotId && item.BinId == binId, cancellationToken)
            ?? throw Conflict("The selected lot and bin do not have an inventory balance.");
        cache.Add(key, balance);
        return balance;
    }

    private static void ValidateCreateOrder(CreateOutboundOrderRequest request)
    {
        if (request.CustomerId <= 0 || request.CurrencyId <= 0
            || request.CreatedByEmployeeId is <= 0
            || request.ExchangeRate <= 0
            || request.ExchangeRate > MaximumExchangeRate
            || !HasScale(request.ExchangeRate, 6))
        {
            throw InvalidInput("Customer, currency, exchange rate, or employee values are invalid.");
        }

        if (request.Lines is null || request.Lines.Count == 0 || request.Lines.Count > 100)
        {
            throw InvalidInput("A sales order must contain between 1 and 100 lines.");
        }

        if (request.Lines.Any(line => line.ProductId <= 0 || line.UnitId <= 0
            || line.Quantity <= 0 || line.Quantity > MaximumQuantity || !HasScale(line.Quantity, 3)
            || line.UnitPrice < 0 || line.UnitPrice > MaximumAmount || !HasScale(line.UnitPrice, 4)
            || line.DiscountAmount < 0 || line.DiscountAmount > MaximumAmount
            || line.DiscountAmount / line.Quantity > line.UnitPrice
            || !HasScale(line.DiscountAmount, 4)))
        {
            throw InvalidInput("Order lines need valid products, units, quantities, prices, and discounts.");
        }

        if (request.Note?.Length > 500)
        {
            throw InvalidInput("The sales-order note cannot exceed 500 characters.");
        }
    }

    private static void ValidateConfirmRequest(ConfirmOutboundOrderRequest request)
    {
        if (request.WarehouseId <= 0 || request.DeliveryAddressId is <= 0
            || request.Allocations is null || request.Allocations.Count == 0
            || request.Allocations.Count > 500)
        {
            throw InvalidInput("Select a warehouse and provide at least one stock allocation.");
        }

        if (request.Allocations.Any(item => item.SalesOrderDetailId <= 0 || item.LotId <= 0
            || item.BinId <= 0 || item.Quantity <= 0 || item.Quantity > MaximumQuantity
            || !HasScale(item.Quantity, 3)))
        {
            throw InvalidInput("Every allocation must reference a line, lot, and bin with a positive quantity.");
        }
    }

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page < 1 || pageSize < 1 || pageSize > MaximumPageSize)
        {
            throw InvalidInput($"Page must be positive and page size must be between 1 and {MaximumPageSize}.");
        }
    }

    private static bool IsOrderStatus(string status)
    {
        return status is Draft or Confirmed or PartiallyDelivered or Delivered or Cancelled;
    }

    private static bool IsDeliveryStatus(string status)
    {
        return status is DeliveryDraft or Picking or Shipped or DeliveryDelivered or "Failed" or DeliveryCancelled;
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

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }

    private static bool HasScale(decimal value, int scale)
    {
        return value == decimal.Round(value, scale);
    }

    private static decimal ToBaseQuantity(decimal quantity, decimal conversionToBase)
    {
        if (conversionToBase <= 0 || quantity > MaximumQuantity / conversionToBase)
        {
            throw InvalidInput("The converted inventory quantity exceeds supported precision.");
        }

        var result = decimal.Round(quantity * conversionToBase, 3, MidpointRounding.AwayFromZero);
        if (result <= 0 || result > MaximumQuantity)
        {
            throw InvalidInput("The converted inventory quantity exceeds supported precision.");
        }

        return result;
    }

    private static bool IsSqlError(DbUpdateException exception, params int[] numbers)
    {
        return exception.GetBaseException() is SqlException sqlException
            && numbers.Contains(sqlException.Number);
    }

    private static OutboundOperationException InvalidInput(string detail)
    {
        return new OutboundOperationException(StatusCodes.Status400BadRequest, "Invalid outbound input", detail);
    }

    private static OutboundOperationException InvalidReference(string detail)
    {
        return new OutboundOperationException(StatusCodes.Status400BadRequest, "Invalid outbound reference", detail);
    }

    private static OutboundOperationException NotFound(string detail)
    {
        return new OutboundOperationException(StatusCodes.Status404NotFound, "Resource not found", detail);
    }

    private static OutboundOperationException Conflict(string detail)
    {
        return new OutboundOperationException(StatusCodes.Status409Conflict, "Outbound conflict", detail);
    }
}
