using System.Reflection;
using CommercialWms.Api.Contracts.Inventory;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using CommercialWms.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace CommercialWms.Api.Tests;

public sealed class InventoryServiceTests
{
    [Fact]
    public async Task AdjustmentIncrease_UpdatesBalanceAndWritesReasonedLedgerEntry()
    {
        await using var context = await CreateSeededContextAsync();
        var service = new InventoryService(context);

        var transaction = await service.CreateAdjustmentAsync(
            new CreateInventoryAdjustmentRequest
            {
                LotId = 1,
                BinId = 1,
                QuantityChange = 5,
                Reason = "Counted additional stock"
            },
            CancellationToken.None);

        var balance = await context.InventoryBalances.SingleAsync(item => item.BinId == 1);
        Assert.Equal(15m, balance.QuantityOnHand);
        Assert.Equal(2m, balance.QuantityReserved);
        Assert.Equal("Adjustment", transaction.TransactionType);
        Assert.Equal(5m, transaction.QuantityChange);
        Assert.Equal("Counted additional stock", transaction.Note);
        Assert.Equal(1, await context.InventoryTransactions.CountAsync());
    }

    [Fact]
    public async Task AdjustmentDecreaseWithinAvailableQuantity_UpdatesBalanceAndLedger()
    {
        await using var context = await CreateSeededContextAsync();
        var service = new InventoryService(context);

        var transaction = await service.CreateAdjustmentAsync(
            new CreateInventoryAdjustmentRequest
            {
                LotId = 1,
                BinId = 1,
                QuantityChange = -3,
                Reason = "Damaged stock counted"
            },
            CancellationToken.None);

        var balance = await context.InventoryBalances.SingleAsync(item => item.BinId == 1);
        Assert.Equal(7m, balance.QuantityOnHand);
        Assert.Equal(2m, balance.QuantityReserved);
        Assert.Equal(-3m, transaction.QuantityChange);
        Assert.Equal("Damaged stock counted", transaction.Note);
        Assert.Equal(1, await context.InventoryTransactions.CountAsync());
    }

    [Fact]
    public async Task AdjustmentDecreaseBeyondAvailableQuantity_IsRejectedWithoutChanges()
    {
        await using var context = await CreateSeededContextAsync();
        var service = new InventoryService(context);

        var exception = await Record.ExceptionAsync(() => service.CreateAdjustmentAsync(
            new CreateInventoryAdjustmentRequest
            {
                LotId = 1,
                BinId = 1,
                QuantityChange = -9,
                Reason = "Attempt exceeds availability"
            },
            CancellationToken.None));

        AssertInventoryError(exception, 409);
        Assert.Equal(10m, (await context.InventoryBalances.SingleAsync(item => item.BinId == 1)).QuantityOnHand);
        Assert.Empty(await context.InventoryTransactions.ToListAsync());
    }

    [Fact]
    public async Task TransferBetweenBins_UpdatesBothBalancesAndWritesBalancedLinkedEntries()
    {
        await using var context = await CreateSeededContextAsync();
        var service = new InventoryService(context);

        var transactions = await service.CreateTransferAsync(
            new CreateInventoryTransferRequest
            {
                LotId = 1,
                SourceBinId = 1,
                DestinationBinId = 2,
                Quantity = 5,
                Reason = "Replenish second location"
            },
            CancellationToken.None);

        var balances = await context.InventoryBalances.OrderBy(item => item.BinId).ToListAsync();
        Assert.Equal(5m, balances[0].QuantityOnHand);
        Assert.Equal(2m, balances[0].QuantityReserved);
        Assert.Equal(9m, balances[1].QuantityOnHand);
        Assert.Equal(1m, balances[1].QuantityReserved);
        Assert.Equal(14m, balances.Sum(item => item.QuantityOnHand));

        Assert.Equal(2, transactions.Count);
        var transferOut = Assert.Single(transactions, item => item.TransactionType == "TransferOut");
        var transferIn = Assert.Single(transactions, item => item.TransactionType == "TransferIn");
        Assert.Equal(-5m, transferOut.QuantityChange);
        Assert.Equal(5m, transferIn.QuantityChange);
        Assert.Equal(1, transferOut.BinId);
        Assert.Equal(2, transferIn.BinId);
        Assert.Equal(1, transferOut.WarehouseId);
        Assert.Equal(2, transferIn.WarehouseId);
        Assert.Equal("InventoryTransfer", transferOut.ReferenceType);
        Assert.Equal("InventoryTransfer", transferIn.ReferenceType);
        Assert.Equal(transferOut.TransactionId, transferOut.ReferenceId);
        Assert.Equal(transferOut.TransactionId, transferIn.ReferenceId);
        Assert.Equal(0m, transactions.Sum(item => item.QuantityChange));

        var ledger = await context.InventoryTransactions.OrderBy(item => item.TransactionId).ToListAsync();
        Assert.Equal(2, ledger.Count);
        Assert.All(ledger, entry => Assert.Contains("Replenish second location", entry.Note));
    }

    [Fact]
    public async Task TransferToSameBin_IsRejectedWithoutChanges()
    {
        await using var context = await CreateSeededContextAsync();
        var service = new InventoryService(context);

        var exception = await Record.ExceptionAsync(() => service.CreateTransferAsync(
            new CreateInventoryTransferRequest
            {
                LotId = 1,
                SourceBinId = 1,
                DestinationBinId = 1,
                Quantity = 1,
                Reason = "Same location transfer"
            },
            CancellationToken.None));

        AssertInventoryError(exception, 400);
        Assert.Equal(10m, (await context.InventoryBalances.SingleAsync(item => item.BinId == 1)).QuantityOnHand);
        Assert.Empty(await context.InventoryTransactions.ToListAsync());
    }

    [Fact]
    public async Task TransferBeyondAvailableQuantity_IsRejectedWithoutChanges()
    {
        await using var context = await CreateSeededContextAsync();
        var service = new InventoryService(context);

        var exception = await Record.ExceptionAsync(() => service.CreateTransferAsync(
            new CreateInventoryTransferRequest
            {
                LotId = 1,
                SourceBinId = 1,
                DestinationBinId = 2,
                Quantity = 9,
                Reason = "Attempt exceeds availability"
            },
            CancellationToken.None));

        AssertInventoryError(exception, 409);
        var balances = await context.InventoryBalances.OrderBy(item => item.BinId).ToListAsync();
        Assert.Equal(new[] { 10m, 4m }, balances.Select(item => item.QuantityOnHand));
        Assert.Empty(await context.InventoryTransactions.ToListAsync());
    }

    private static void AssertInventoryError(Exception? exception, int expectedStatusCode)
    {
        Assert.NotNull(exception);
        Assert.Equal("InventoryOperationException", exception.GetType().Name);
        var statusCode = exception.GetType()
            .GetProperty("StatusCode", BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(exception);
        Assert.Equal(expectedStatusCode, statusCode);
    }

    private static async Task<CommercialWmsDbContext> CreateSeededContextAsync()
    {
        var options = new DbContextOptionsBuilder<CommercialWmsDbContext>()
            .UseInMemoryDatabase($"InventoryTests-{Guid.NewGuid():N}")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var context = new CommercialWmsDbContext(options);
        context.Warehouses.AddRange(
            new Warehouse
            {
                WarehouseId = 1,
                WarehouseCode = "WH-ONE",
                WarehouseName = "Test Warehouse One",
                Address = "Isolated test data"
            },
            new Warehouse
            {
                WarehouseId = 2,
                WarehouseCode = "WH-TWO",
                WarehouseName = "Test Warehouse Two",
                Address = "Isolated test data"
            });
        context.WarehouseZones.AddRange(
            new WarehouseZone { ZoneId = 1, WarehouseId = 1, ZoneCode = "Z1", ZoneName = "Zone One" },
            new WarehouseZone { ZoneId = 2, WarehouseId = 2, ZoneCode = "Z2", ZoneName = "Zone Two" });
        context.WarehouseAisles.AddRange(
            new WarehouseAisle { AisleId = 1, ZoneId = 1, AisleCode = "A1" },
            new WarehouseAisle { AisleId = 2, ZoneId = 2, AisleCode = "A2" });
        context.WarehouseRacks.AddRange(
            new WarehouseRack { RackId = 1, AisleId = 1, RackCode = "R1" },
            new WarehouseRack { RackId = 2, AisleId = 2, RackCode = "R2" });
        context.WarehouseShelves.AddRange(
            new WarehouseShelf { ShelfId = 1, RackId = 1, ShelfCode = "S1" },
            new WarehouseShelf { ShelfId = 2, RackId = 2, ShelfCode = "S2" });
        context.WarehouseBins.AddRange(
            new WarehouseBin { BinId = 1, ShelfId = 1, BinCode = "B1", IsActive = true },
            new WarehouseBin { BinId = 2, ShelfId = 2, BinCode = "B2", IsActive = true });
        context.Countries.Add(new Country
        {
            CountryId = 1,
            CountryCode = "TST",
            CountryName = "Test Origin"
        });
        context.Manufacturers.Add(new Manufacturer
        {
            ManufacturerId = 1,
            ManufacturerCode = "MFG-TST",
            ManufacturerName = "Test Manufacturer",
            CountryId = 1
        });
        context.Products.Add(new Product
        {
            ProductId = 1,
            SKU = "SKU-TST",
            ProductName = "Test Product",
            CategoryId = 1
        });
        context.InventoryLots.Add(new InventoryLot
        {
            LotId = 1,
            LotCode = "LOT-TST",
            ProductId = 1,
            ManufacturerId = 1,
            CountryId = 1
        });
        context.InventoryBalances.AddRange(
            new InventoryBalance
            {
                BalanceId = 1,
                LotId = 1,
                BinId = 1,
                QuantityOnHand = 10,
                QuantityReserved = 2
            },
            new InventoryBalance
            {
                BalanceId = 2,
                LotId = 1,
                BinId = 2,
                QuantityOnHand = 4,
                QuantityReserved = 1
            });
        await context.SaveChangesAsync();
        return context;
    }
}
