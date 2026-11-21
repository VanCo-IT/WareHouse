using System.Data;
using CommercialWms.Api.Contracts.Inventory;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Services;

internal sealed class InventoryOperationException(int statusCode, string title, string detail) : Exception(detail)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title;
}

public sealed class InventoryService(CommercialWmsDbContext dbContext) : IInventoryService
{
    private const int MaximumPageSize = 100;
    private const decimal MaximumInventoryQuantity = 999_999_999_999_999.999m;

    public async Task<InventoryStockPageResponse> GetStockAsync(
        string? search,
        int? warehouseId,
        int? productId,
        int? binId,
        bool availableOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ValidatePaging(page, pageSize);
        ValidatePositiveFilter(warehouseId, "Warehouse");
        ValidatePositiveFilter(productId, "Product");
        ValidatePositiveFilter(binId, "Bin");

        var query =
            from balance in dbContext.InventoryBalances.AsNoTracking()
            join lot in dbContext.InventoryLots.AsNoTracking() on balance.LotId equals lot.LotId
            join product in dbContext.Products.AsNoTracking() on lot.ProductId equals product.ProductId
            join manufacturer in dbContext.Manufacturers.AsNoTracking()
                on lot.ManufacturerId equals manufacturer.ManufacturerId
            join country in dbContext.Countries.AsNoTracking() on lot.CountryId equals country.CountryId
            join bin in dbContext.WarehouseBins.AsNoTracking() on balance.BinId equals bin.BinId
            join shelf in dbContext.WarehouseShelves.AsNoTracking() on bin.ShelfId equals shelf.ShelfId
            join rack in dbContext.WarehouseRacks.AsNoTracking() on shelf.RackId equals rack.RackId
            join aisle in dbContext.WarehouseAisles.AsNoTracking() on rack.AisleId equals aisle.AisleId
            join zone in dbContext.WarehouseZones.AsNoTracking() on aisle.ZoneId equals zone.ZoneId
            join warehouse in dbContext.Warehouses.AsNoTracking() on zone.WarehouseId equals warehouse.WarehouseId
            select new
            {
                balance.BalanceId,
                balance.LotId,
                lot.LotCode,
                lot.ProductId,
                product.SKU,
                product.ProductName,
                warehouse.WarehouseId,
                warehouse.WarehouseCode,
                warehouse.WarehouseName,
                bin.BinId,
                bin.BinCode,
                LocationCode = zone.ZoneCode + "-" + aisle.AisleCode + "-" + rack.RackCode + "-" + shelf.ShelfCode,
                ManufacturerName = manufacturer.ManufacturerName,
                CountryName = country.CountryName,
                lot.ManufactureDate,
                lot.ExpiryDate,
                balance.QuantityOnHand,
                balance.QuantityReserved,
                balance.UpdatedAt
            };

        if (warehouseId.HasValue)
        {
            query = query.Where(row => row.WarehouseId == warehouseId.Value);
        }

        if (productId.HasValue)
        {
            query = query.Where(row => row.ProductId == productId.Value);
        }

        if (binId.HasValue)
        {
            query = query.Where(row => row.BinId == binId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(row => row.SKU.Contains(value)
                || row.ProductName.Contains(value)
                || row.LotCode.Contains(value)
                || row.BinCode.Contains(value)
                || row.WarehouseCode.Contains(value)
                || row.WarehouseName.Contains(value));
        }

        if (availableOnly)
        {
            query = query.Where(row => row.QuantityOnHand > row.QuantityReserved);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalOnHand = await query.SumAsync(row => (decimal?)row.QuantityOnHand, cancellationToken) ?? 0;
        var totalReserved = await query.SumAsync(row => (decimal?)row.QuantityReserved, cancellationToken) ?? 0;
        var rows = await query
            .OrderBy(row => row.WarehouseCode)
            .ThenBy(row => row.BinCode)
            .ThenBy(row => row.SKU)
            .ThenBy(row => row.LotCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = rows.Select(row => new InventoryStockResponse(
            row.BalanceId,
            row.LotId,
            row.LotCode,
            row.ProductId,
            row.SKU,
            row.ProductName,
            row.WarehouseId,
            row.WarehouseCode,
            row.WarehouseName,
            row.BinId,
            row.BinCode,
            row.LocationCode,
            row.ManufacturerName,
            row.CountryName,
            row.ManufactureDate,
            row.ExpiryDate,
            row.QuantityOnHand,
            row.QuantityReserved,
            row.QuantityOnHand - row.QuantityReserved,
            ToUtcOffset(row.UpdatedAt)))
            .ToArray();

        return new InventoryStockPageResponse(
            items,
            page,
            pageSize,
            totalCount,
            totalOnHand,
            totalReserved,
            totalOnHand - totalReserved);
    }

    public async Task<InventoryTransactionPageResponse> GetTransactionsAsync(
        int? warehouseId,
        int? productId,
        string? transactionType,
        DateOnly? fromDate,
        DateOnly? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ValidatePaging(page, pageSize);
        ValidatePositiveFilter(warehouseId, "Warehouse");
        ValidatePositiveFilter(productId, "Product");
        if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
        {
            throw InvalidInput("The from date must not be later than the to date.");
        }

        var query =
            from movement in dbContext.InventoryTransactions.AsNoTracking()
            join lot in dbContext.InventoryLots.AsNoTracking() on movement.LotId equals lot.LotId
            join product in dbContext.Products.AsNoTracking() on lot.ProductId equals product.ProductId
            join warehouse in dbContext.Warehouses.AsNoTracking() on movement.WarehouseId equals warehouse.WarehouseId
            join binGroup in dbContext.WarehouseBins.AsNoTracking()
                on movement.BinId equals binGroup.BinId into bins
            from bin in bins.DefaultIfEmpty()
            select new
            {
                movement.TransactionId,
                movement.LotId,
                lot.LotCode,
                lot.ProductId,
                product.SKU,
                product.ProductName,
                movement.WarehouseId,
                warehouse.WarehouseCode,
                warehouse.WarehouseName,
                movement.BinId,
                BinCode = bin == null ? null : bin.BinCode,
                movement.TransactionType,
                movement.QuantityChange,
                movement.ReferenceType,
                movement.ReferenceId,
                movement.OccurredAt,
                movement.Note
            };

        if (warehouseId.HasValue)
        {
            query = query.Where(row => row.WarehouseId == warehouseId.Value);
        }

        if (productId.HasValue)
        {
            query = query.Where(row => row.ProductId == productId.Value);
        }

        if (!string.IsNullOrWhiteSpace(transactionType))
        {
            var normalizedType = transactionType.Trim();
            if (!IsTransactionType(normalizedType))
            {
                throw InvalidInput("The inventory transaction type is not supported.");
            }

            query = query.Where(row => row.TransactionType == normalizedType);
        }

        if (fromDate.HasValue)
        {
            var start = fromDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(row => row.OccurredAt >= start);
        }

        if (toDate.HasValue && toDate.Value < DateOnly.MaxValue)
        {
            var endExclusive = toDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            query = query.Where(row => row.OccurredAt < endExclusive);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(row => row.OccurredAt)
            .ThenByDescending(row => row.TransactionId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = rows.Select(row => new InventoryTransactionResponse(
            row.TransactionId,
            row.LotId,
            row.LotCode,
            row.ProductId,
            row.SKU,
            row.ProductName,
            row.WarehouseId,
            row.WarehouseCode,
            row.WarehouseName,
            row.BinId,
            row.BinCode,
            row.TransactionType,
            row.QuantityChange,
            row.ReferenceType,
            row.ReferenceId,
            ToUtcOffset(row.OccurredAt),
            row.Note))
            .ToArray();

        return new InventoryTransactionPageResponse(items, page, pageSize, totalCount);
    }

    public async Task<InventoryTransactionResponse> CreateAdjustmentAsync(
        CreateInventoryAdjustmentRequest request,
        CancellationToken cancellationToken)
    {
        ValidateQuantity(request.QuantityChange, allowNegative: true);
        var reason = NormalizeReason(request.Reason);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var lot = await dbContext.InventoryLots
            .SingleOrDefaultAsync(item => item.LotId == request.LotId, cancellationToken)
            ?? throw NotFound("The inventory lot was not found.");
        var warehouseId = await GetActiveBinWarehouseIdAsync(request.BinId, cancellationToken);
        await ValidateEmployeeAsync(request.CreatedByEmployeeId, cancellationToken);

        var balance = await dbContext.InventoryBalances
            .SingleOrDefaultAsync(
                item => item.LotId == request.LotId && item.BinId == request.BinId,
                cancellationToken);
        if (balance is null)
        {
            if (request.QuantityChange < 0)
            {
                throw Conflict("There is no stock in the selected lot and bin to decrease.");
            }

            balance = new InventoryBalance
            {
                LotId = lot.LotId,
                BinId = request.BinId,
                QuantityOnHand = 0,
                QuantityReserved = 0,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.InventoryBalances.Add(balance);
        }

        var newOnHand = balance.QuantityOnHand + request.QuantityChange;
        if (newOnHand < balance.QuantityReserved || newOnHand > MaximumInventoryQuantity)
        {
            throw Conflict("The adjustment would violate the reserved or maximum on-hand quantity.");
        }

        var now = DateTime.UtcNow;
        balance.QuantityOnHand = newOnHand;
        balance.UpdatedAt = now;
        var movement = new InventoryTransaction
        {
            LotId = lot.LotId,
            WarehouseId = warehouseId,
            BinId = request.BinId,
            TransactionType = "Adjustment",
            QuantityChange = request.QuantityChange,
            ReferenceType = "Adjustment",
            OccurredAt = now,
            CreatedByEmployeeId = request.CreatedByEmployeeId,
            Note = reason
        };
        dbContext.InventoryTransactions.Add(movement);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsSqlError(exception, 547))
        {
            throw InvalidReference("The lot, bin, warehouse, or employee reference is no longer valid.");
        }

        return await GetTransactionResponseAsync(movement.TransactionId, cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryTransactionResponse>> CreateTransferAsync(
        CreateInventoryTransferRequest request,
        CancellationToken cancellationToken)
    {
        ValidateQuantity(request.Quantity, allowNegative: false);
        if (request.SourceBinId == request.DestinationBinId)
        {
            throw InvalidInput("Source and destination bins must be different.");
        }

        var reason = NormalizeReason(request.Reason);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var lot = await dbContext.InventoryLots
            .SingleOrDefaultAsync(item => item.LotId == request.LotId, cancellationToken)
            ?? throw NotFound("The inventory lot was not found.");
        var sourceWarehouseId = await GetActiveBinWarehouseIdAsync(request.SourceBinId, cancellationToken);
        var destinationWarehouseId = await GetActiveBinWarehouseIdAsync(request.DestinationBinId, cancellationToken);
        await ValidateEmployeeAsync(request.CreatedByEmployeeId, cancellationToken);

        var source = await dbContext.InventoryBalances
            .SingleOrDefaultAsync(
                item => item.LotId == request.LotId && item.BinId == request.SourceBinId,
                cancellationToken)
            ?? throw Conflict("There is no stock in the selected source bin.");
        if (source.QuantityOnHand - source.QuantityReserved < request.Quantity)
        {
            throw Conflict("Transfer quantity exceeds the available stock in the source bin.");
        }

        var destination = await dbContext.InventoryBalances
            .SingleOrDefaultAsync(
                item => item.LotId == request.LotId && item.BinId == request.DestinationBinId,
                cancellationToken);
        if (destination is null)
        {
            destination = new InventoryBalance
            {
                LotId = lot.LotId,
                BinId = request.DestinationBinId,
                QuantityOnHand = 0,
                QuantityReserved = 0,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.InventoryBalances.Add(destination);
        }

        if (destination.QuantityOnHand + request.Quantity > MaximumInventoryQuantity)
        {
            throw Conflict("The transfer would exceed the destination bin's maximum on-hand quantity.");
        }

        var now = DateTime.UtcNow;
        source.QuantityOnHand -= request.Quantity;
        source.UpdatedAt = now;
        destination.QuantityOnHand += request.Quantity;
        destination.UpdatedAt = now;
        var reference = $"Stock transfer: {reason}";
        var transferOut = new InventoryTransaction
        {
            LotId = lot.LotId,
            WarehouseId = sourceWarehouseId,
            BinId = request.SourceBinId,
            TransactionType = "TransferOut",
            QuantityChange = -request.Quantity,
            ReferenceType = "InventoryTransfer",
            OccurredAt = now,
            CreatedByEmployeeId = request.CreatedByEmployeeId,
            Note = reference
        };
        var transferIn = new InventoryTransaction
        {
            LotId = lot.LotId,
            WarehouseId = destinationWarehouseId,
            BinId = request.DestinationBinId,
            TransactionType = "TransferIn",
            QuantityChange = request.Quantity,
            ReferenceType = "InventoryTransfer",
            OccurredAt = now,
            CreatedByEmployeeId = request.CreatedByEmployeeId,
            Note = reference
        };
        dbContext.InventoryTransactions.AddRange(transferOut, transferIn);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            transferOut.ReferenceId = transferOut.TransactionId;
            transferIn.ReferenceId = transferOut.TransactionId;
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsSqlError(exception, 2601, 2627))
        {
            throw Conflict("A unique inventory balance already exists. The transfer was not committed.");
        }
        catch (DbUpdateException exception) when (IsSqlError(exception, 547))
        {
            throw InvalidReference("The lot, bin, warehouse, or employee reference is no longer valid.");
        }

        return
        [
            await GetTransactionResponseAsync(transferOut.TransactionId, cancellationToken),
            await GetTransactionResponseAsync(transferIn.TransactionId, cancellationToken)
        ];
    }

    private async Task<InventoryTransactionResponse> GetTransactionResponseAsync(
        long transactionId,
        CancellationToken cancellationToken)
    {
        var row = await (
            from movement in dbContext.InventoryTransactions.AsNoTracking()
            join lot in dbContext.InventoryLots.AsNoTracking() on movement.LotId equals lot.LotId
            join product in dbContext.Products.AsNoTracking() on lot.ProductId equals product.ProductId
            join warehouse in dbContext.Warehouses.AsNoTracking()
                on movement.WarehouseId equals warehouse.WarehouseId
            join binGroup in dbContext.WarehouseBins.AsNoTracking()
                on movement.BinId equals binGroup.BinId into bins
            from bin in bins.DefaultIfEmpty()
            where movement.TransactionId == transactionId
            select new
            {
                movement.TransactionId,
                movement.LotId,
                lot.LotCode,
                lot.ProductId,
                product.SKU,
                product.ProductName,
                movement.WarehouseId,
                warehouse.WarehouseCode,
                warehouse.WarehouseName,
                movement.BinId,
                BinCode = bin == null ? null : bin.BinCode,
                movement.TransactionType,
                movement.QuantityChange,
                movement.ReferenceType,
                movement.ReferenceId,
                movement.OccurredAt,
                movement.Note
            }).SingleAsync(cancellationToken);

        return new InventoryTransactionResponse(
            row.TransactionId,
            row.LotId,
            row.LotCode,
            row.ProductId,
            row.SKU,
            row.ProductName,
            row.WarehouseId,
            row.WarehouseCode,
            row.WarehouseName,
            row.BinId,
            row.BinCode,
            row.TransactionType,
            row.QuantityChange,
            row.ReferenceType,
            row.ReferenceId,
            ToUtcOffset(row.OccurredAt),
            row.Note);
    }

    private async Task<int> GetActiveBinWarehouseIdAsync(int binId, CancellationToken cancellationToken)
    {
        var warehouseId = await (
            from bin in dbContext.WarehouseBins.AsNoTracking()
            join shelf in dbContext.WarehouseShelves.AsNoTracking() on bin.ShelfId equals shelf.ShelfId
            join rack in dbContext.WarehouseRacks.AsNoTracking() on shelf.RackId equals rack.RackId
            join aisle in dbContext.WarehouseAisles.AsNoTracking() on rack.AisleId equals aisle.AisleId
            join zone in dbContext.WarehouseZones.AsNoTracking() on aisle.ZoneId equals zone.ZoneId
            join warehouse in dbContext.Warehouses.AsNoTracking() on zone.WarehouseId equals warehouse.WarehouseId
            where bin.BinId == binId && bin.IsActive && warehouse.IsActive
            select (int?)warehouse.WarehouseId)
            .SingleOrDefaultAsync(cancellationToken);

        return warehouseId ?? throw InvalidReference("Select an active bin in an active warehouse.");
    }

    private async Task ValidateEmployeeAsync(int? employeeId, CancellationToken cancellationToken)
    {
        if (employeeId.HasValue
            && !await dbContext.Employees.AnyAsync(
                employee => employee.EmployeeId == employeeId.Value && employee.IsActive,
                cancellationToken))
        {
            throw InvalidReference("Select an active employee.");
        }
    }

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page < 1
            || pageSize < 1
            || pageSize > MaximumPageSize
            || page > int.MaxValue / pageSize)
        {
            throw InvalidInput($"Page must be positive and page size must be between 1 and {MaximumPageSize}.");
        }
    }

    private static void ValidatePositiveFilter(int? value, string name)
    {
        if (value is <= 0)
        {
            throw InvalidInput($"{name} filter must be a positive identifier.");
        }
    }

    private static void ValidateQuantity(decimal quantity, bool allowNegative)
    {
        if (quantity == 0
            || (!allowNegative && quantity < 0)
            || quantity > MaximumInventoryQuantity
            || quantity < -MaximumInventoryQuantity
            || decimal.Round(quantity, 3) != quantity)
        {
            throw InvalidInput("Quantity must be non-zero, have at most three decimal places, and use the permitted sign.");
        }
    }

    private static string NormalizeReason(string reason)
    {
        var value = reason.Trim();
        if (value.Length is < 3 or > 300)
        {
            throw InvalidInput("A reason between 3 and 300 characters is required.");
        }

        return value;
    }

    private static bool IsTransactionType(string value)
    {
        return value is "Receipt" or "Issue" or "TransferOut" or "TransferIn" or "Adjustment" or "Return";
    }

    private static DateTimeOffset ToUtcOffset(DateTime value)
    {
        return new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }

    private static InventoryOperationException InvalidInput(string detail)
    {
        return new InventoryOperationException(StatusCodes.Status400BadRequest, "Invalid inventory request", detail);
    }

    private static InventoryOperationException InvalidReference(string detail)
    {
        return new InventoryOperationException(StatusCodes.Status400BadRequest, "Invalid inventory reference", detail);
    }

    private static InventoryOperationException NotFound(string detail)
    {
        return new InventoryOperationException(StatusCodes.Status404NotFound, "Inventory record not found", detail);
    }

    private static InventoryOperationException Conflict(string detail)
    {
        return new InventoryOperationException(StatusCodes.Status409Conflict, "Inventory operation conflict", detail);
    }

    private static bool IsSqlError(DbUpdateException exception, params int[] numbers)
    {
        return exception.GetBaseException() is Microsoft.Data.SqlClient.SqlException sqlException
            && numbers.Contains(sqlException.Number);
    }
}
