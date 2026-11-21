using System.Reflection;
using CommercialWms.Api.Contracts.Outbound;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using CommercialWms.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace CommercialWms.Api.Tests;

public sealed class OutboundServiceTests
{
    [Fact]
    public async Task OrderAllocationAndShipment_ReserveThenIssueStockOnce()
    {
        await using var context = await CreateSeededContextAsync();
        var service = new OutboundService(context);
        var order = await CreateOrderAsync(service, quantity: 3);

        var reservedOrder = await service.ConfirmOrderAsync(
            order.SalesOrderId,
            new ConfirmOutboundOrderRequest(
                WarehouseId: 1,
                DeliveryAddressId: null,
                [new OutboundAllocationRequest(
                    order.Lines[0].SalesOrderDetailId,
                    LotId: 1,
                    BinId: 1,
                    Quantity: 3)]),
            CancellationToken.None);

        Assert.Equal("Confirmed", reservedOrder.Status);
        Assert.Equal(8m, (await context.InventoryBalances.SingleAsync()).QuantityOnHand);
        Assert.Equal(6m, (await context.InventoryBalances.SingleAsync()).QuantityReserved);
        Assert.Equal(6m, (await context.StockReservations.SingleAsync()).ReservedQuantity);
        var delivery = Assert.Single(await context.Deliveries.ToListAsync());
        Assert.Equal("Draft", delivery.Status);

        await service.PrepareDeliveryAsync(delivery.DeliveryId, CancellationToken.None);
        var shipped = await service.ShipDeliveryAsync(
            delivery.DeliveryId,
            new ShipOutboundDeliveryRequest(null),
            CancellationToken.None);

        var balance = await context.InventoryBalances.SingleAsync();
        var reservation = await context.StockReservations.SingleAsync();
        var transaction = await context.InventoryTransactions.SingleAsync();
        Assert.Equal("Shipped", shipped.Status);
        Assert.Equal("Delivered", (await context.SalesOrders.SingleAsync()).Status);
        Assert.Equal(2m, balance.QuantityOnHand);
        Assert.Equal(0m, balance.QuantityReserved);
        Assert.Equal(0m, reservation.ReservedQuantity);
        Assert.Equal("Fulfilled", reservation.Status);
        Assert.Equal("Issue", transaction.TransactionType);
        Assert.Equal(-6m, transaction.QuantityChange);
        Assert.Equal("Delivery", transaction.ReferenceType);
        Assert.Equal(delivery.DeliveryId, transaction.ReferenceId);
    }

    [Fact]
    public async Task AllocationBeyondAvailableStock_IsRejectedWithoutReservationOrDelivery()
    {
        await using var context = await CreateSeededContextAsync();
        var service = new OutboundService(context);
        var order = await CreateOrderAsync(service, quantity: 5);

        var exception = await Record.ExceptionAsync(() => service.ConfirmOrderAsync(
            order.SalesOrderId,
            new ConfirmOutboundOrderRequest(
                1,
                null,
                [new OutboundAllocationRequest(order.Lines[0].SalesOrderDetailId, 1, 1, 5)]),
            CancellationToken.None));

        AssertOutboundError(exception, 409);
        var balance = await context.InventoryBalances.SingleAsync();
        Assert.Equal(8m, balance.QuantityOnHand);
        Assert.Equal(0m, balance.QuantityReserved);
        Assert.Empty(await context.StockReservations.ToListAsync());
        Assert.Empty(await context.Deliveries.ToListAsync());
    }

    [Fact]
    public async Task AllocationMustExactlyCoverUndeliveredOrderLine()
    {
        await using var context = await CreateSeededContextAsync();
        var service = new OutboundService(context);
        var order = await CreateOrderAsync(service, quantity: 3);

        var exception = await Record.ExceptionAsync(() => service.ConfirmOrderAsync(
            order.SalesOrderId,
            new ConfirmOutboundOrderRequest(
                1,
                null,
                [new OutboundAllocationRequest(order.Lines[0].SalesOrderDetailId, 1, 1, 2)]),
            CancellationToken.None));

        AssertOutboundError(exception, 400);
        Assert.Empty(await context.Deliveries.ToListAsync());
        Assert.Empty(await context.StockReservations.ToListAsync());
    }

    [Fact]
    public async Task CancellingUnshippedDelivery_ReleasesReservationWithoutChangingOnHand()
    {
        await using var context = await CreateSeededContextAsync();
        var service = new OutboundService(context);
        var order = await CreateOrderAsync(service, quantity: 3);
        var confirmed = await service.ConfirmOrderAsync(
            order.SalesOrderId,
            new ConfirmOutboundOrderRequest(
                1,
                null,
                [new OutboundAllocationRequest(order.Lines[0].SalesOrderDetailId, 1, 1, 3)]),
            CancellationToken.None);
        var deliveryId = Assert.Single(confirmed.Deliveries).DeliveryId;

        var cancelled = await service.CancelDeliveryAsync(deliveryId, CancellationToken.None);

        var balance = await context.InventoryBalances.SingleAsync();
        var reservation = await context.StockReservations.SingleAsync();
        Assert.Equal("Cancelled", cancelled.Status);
        Assert.Equal("Draft", (await context.SalesOrders.SingleAsync()).Status);
        Assert.Equal(8m, balance.QuantityOnHand);
        Assert.Equal(0m, balance.QuantityReserved);
        Assert.Equal(0m, reservation.ReservedQuantity);
        Assert.Equal("Released", reservation.Status);
        Assert.Empty(await context.InventoryTransactions.ToListAsync());
    }

    [Fact]
    public async Task ShippedDeliveryCannotBeShippedTwice()
    {
        await using var context = await CreateSeededContextAsync();
        var service = new OutboundService(context);
        var order = await CreateOrderAsync(service, quantity: 2);
        var confirmed = await service.ConfirmOrderAsync(
            order.SalesOrderId,
            new ConfirmOutboundOrderRequest(
                1,
                null,
                [new OutboundAllocationRequest(order.Lines[0].SalesOrderDetailId, 1, 1, 2)]),
            CancellationToken.None);
        var deliveryId = Assert.Single(confirmed.Deliveries).DeliveryId;
        await service.PrepareDeliveryAsync(deliveryId, CancellationToken.None);
        await service.ShipDeliveryAsync(deliveryId, new ShipOutboundDeliveryRequest(null), CancellationToken.None);

        var exception = await Record.ExceptionAsync(() => service.ShipDeliveryAsync(
            deliveryId,
            new ShipOutboundDeliveryRequest(null),
            CancellationToken.None));

        AssertOutboundError(exception, 409);
        Assert.Equal(4m, (await context.InventoryBalances.SingleAsync()).QuantityOnHand);
        Assert.Single(await context.InventoryTransactions.ToListAsync());
    }

    [Fact]
    public async Task AllocationFromAnotherWarehouse_IsRejectedWithoutReservingStock()
    {
        await using var context = await CreateSeededContextAsync();
        context.Warehouses.Add(new Warehouse
        {
            WarehouseId = 2,
            WarehouseCode = "WH-OTHER",
            WarehouseName = "Other Test Warehouse",
            Address = "Isolated test data"
        });
        context.WarehouseZones.Add(new WarehouseZone
        {
            ZoneId = 2,
            WarehouseId = 2,
            ZoneCode = "Z2",
            ZoneName = "Other Test Zone"
        });
        context.WarehouseAisles.Add(new WarehouseAisle { AisleId = 2, ZoneId = 2, AisleCode = "A2" });
        context.WarehouseRacks.Add(new WarehouseRack { RackId = 2, AisleId = 2, RackCode = "R2" });
        context.WarehouseShelves.Add(new WarehouseShelf { ShelfId = 2, RackId = 2, ShelfCode = "S2" });
        context.WarehouseBins.Add(new WarehouseBin
        {
            BinId = 2,
            ShelfId = 2,
            BinCode = "B2",
            IsActive = true
        });
        await context.SaveChangesAsync();
        var service = new OutboundService(context);
        var order = await CreateOrderAsync(service, quantity: 2);

        var exception = await Record.ExceptionAsync(() => service.ConfirmOrderAsync(
            order.SalesOrderId,
            new ConfirmOutboundOrderRequest(
                WarehouseId: 1,
                DeliveryAddressId: null,
                [new OutboundAllocationRequest(order.Lines[0].SalesOrderDetailId, 1, 2, 2)]),
            CancellationToken.None));

        AssertOutboundError(exception, 400);
        Assert.Equal(0m, (await context.InventoryBalances.SingleAsync()).QuantityReserved);
        Assert.Empty(await context.Deliveries.ToListAsync());
    }

    [Fact]
    public async Task DraftOrderCanBeCancelledWithoutInventoryChanges()
    {
        await using var context = await CreateSeededContextAsync();
        var service = new OutboundService(context);
        var order = await CreateOrderAsync(service, quantity: 2);

        var cancelled = await service.CancelOrderAsync(order.SalesOrderId, CancellationToken.None);

        Assert.Equal("Cancelled", cancelled.Status);
        Assert.Equal(8m, (await context.InventoryBalances.SingleAsync()).QuantityOnHand);
        Assert.Equal(0m, (await context.InventoryBalances.SingleAsync()).QuantityReserved);
        Assert.Empty(await context.InventoryTransactions.ToListAsync());
    }

    private static async Task<OutboundOrderResponse> CreateOrderAsync(OutboundService service, decimal quantity)
    {
        return await service.CreateOrderAsync(
            new CreateOutboundOrderRequest(
                CustomerId: 1,
                CurrencyId: 1,
                ExchangeRate: 1,
                CreatedByEmployeeId: null,
                Note: null,
                [new CreateOutboundOrderLineRequest(
                    ProductId: 1,
                    UnitId: 1,
                    Quantity: quantity,
                    UnitPrice: 12.5m,
                    DiscountAmount: 0)]),
            CancellationToken.None);
    }

    private static void AssertOutboundError(Exception? exception, int expectedStatusCode)
    {
        Assert.NotNull(exception);
        Assert.Equal("OutboundOperationException", exception.GetType().Name);
        var statusCode = exception.GetType()
            .GetProperty("StatusCode", BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(exception);
        Assert.Equal(expectedStatusCode, statusCode);
    }

    private static async Task<CommercialWmsDbContext> CreateSeededContextAsync()
    {
        var options = new DbContextOptionsBuilder<CommercialWmsDbContext>()
            .UseInMemoryDatabase($"OutboundTests-{Guid.NewGuid():N}")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var context = new CommercialWmsDbContext(options);
        context.Customers.Add(new Customer
        {
            CustomerId = 1,
            CustomerCode = "CUS-TEST",
            CustomerName = "Isolated Test Customer",
            CustomerTypeId = 1
        });
        context.Currencies.Add(new Currency
        {
            CurrencyId = 1,
            CurrencyCode = "TST",
            CurrencyName = "Test Currency"
        });
        context.Units.Add(new Unit
        {
            UnitId = 1,
            UnitCode = "BOX",
            UnitName = "Box"
        });
        context.Products.Add(new Product
        {
            ProductId = 1,
            SKU = "SKU-OUTBOUND-TEST",
            ProductName = "Outbound Test Product",
            CategoryId = 1
        });
        context.ProductUnits.Add(new ProductUnit
        {
            ProductUnitId = 1,
            ProductId = 1,
            UnitId = 1,
            ConversionToBase = 2,
            IsBaseUnit = false
        });
        context.Warehouses.Add(new Warehouse
        {
            WarehouseId = 1,
            WarehouseCode = "WH-TEST",
            WarehouseName = "Isolated Test Warehouse",
            Address = "Isolated test data"
        });
        context.WarehouseZones.Add(new WarehouseZone
        {
            ZoneId = 1,
            WarehouseId = 1,
            ZoneCode = "Z1",
            ZoneName = "Test Zone"
        });
        context.WarehouseAisles.Add(new WarehouseAisle { AisleId = 1, ZoneId = 1, AisleCode = "A1" });
        context.WarehouseRacks.Add(new WarehouseRack { RackId = 1, AisleId = 1, RackCode = "R1" });
        context.WarehouseShelves.Add(new WarehouseShelf { ShelfId = 1, RackId = 1, ShelfCode = "S1" });
        context.WarehouseBins.Add(new WarehouseBin
        {
            BinId = 1,
            ShelfId = 1,
            BinCode = "B1",
            IsActive = true
        });
        context.InventoryLots.Add(new InventoryLot
        {
            LotId = 1,
            LotCode = "LOT-OUTBOUND-TEST",
            ProductId = 1,
            ManufacturerId = 1,
            CountryId = 1
        });
        context.InventoryBalances.Add(new InventoryBalance
        {
            BalanceId = 1,
            LotId = 1,
            BinId = 1,
            QuantityOnHand = 8,
            QuantityReserved = 0,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        return context;
    }
}
