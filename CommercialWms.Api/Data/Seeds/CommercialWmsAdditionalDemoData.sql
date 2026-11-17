SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'WareHouse'
BEGIN
    ;THROW 51000, N'This seed can run only in the WareHouse database.', 1;
END;

DECLARE @EmployeeId int;
DECLARE @RetailCustomerTypeId int;
DECLARE @CurrencyId int;
DECLARE @SourceWarehouseId int;
DECLARE @DestinationWarehouseId int;
DECLARE @SourceBinId int;
DECLARE @DestinationBinId int;
DECLARE @BeefProductId int;
DECLARE @ChickenProductId int;
DECLARE @BeefUnitId int;
DECLARE @ChickenUnitId int;
DECLARE @BeefLotId bigint;
DECLARE @ChickenLotId bigint;

SELECT @EmployeeId = EmployeeId
FROM dbo.Employees
WHERE EmployeeCode = 'DEMO-EMP-HCM'
  AND IsActive = 1;

SELECT @RetailCustomerTypeId = CustomerTypeId
FROM dbo.CustomerTypes
WHERE TypeCode = 'RETAIL';

SELECT @CurrencyId = CurrencyId
FROM dbo.Currencies
WHERE CurrencyCode = 'VND';

SELECT @SourceWarehouseId = WarehouseId
FROM dbo.Warehouses
WHERE WarehouseCode = 'DEMO-WH-HCM';

SELECT @DestinationWarehouseId = WarehouseId
FROM dbo.Warehouses
WHERE WarehouseCode = 'DEMO-WH-DAD';

SELECT @SourceBinId = b.BinId
FROM dbo.WarehouseBins b
JOIN dbo.WarehouseShelves s ON s.ShelfId = b.ShelfId
JOIN dbo.WarehouseRacks r ON r.RackId = s.RackId
JOIN dbo.WarehouseAisles a ON a.AisleId = r.AisleId
JOIN dbo.WarehouseZones z ON z.ZoneId = a.ZoneId
WHERE z.WarehouseId = @SourceWarehouseId
  AND z.ZoneCode = 'COLD'
  AND b.BinCode = 'B01';

SELECT @DestinationBinId = b.BinId
FROM dbo.WarehouseBins b
JOIN dbo.WarehouseShelves s ON s.ShelfId = b.ShelfId
JOIN dbo.WarehouseRacks r ON r.RackId = s.RackId
JOIN dbo.WarehouseAisles a ON a.AisleId = r.AisleId
JOIN dbo.WarehouseZones z ON z.ZoneId = a.ZoneId
WHERE z.WarehouseId = @DestinationWarehouseId
  AND z.ZoneCode = 'COLD'
  AND b.BinCode = 'B01';

SELECT @BeefProductId = p.ProductId,
       @BeefUnitId = pu.UnitId
FROM dbo.Products p
JOIN dbo.ProductUnits pu ON pu.ProductId = p.ProductId
WHERE p.SKU = 'DEMO-BEEF-001'
  AND pu.IsBaseUnit = 1;

SELECT @ChickenProductId = p.ProductId,
       @ChickenUnitId = pu.UnitId
FROM dbo.Products p
JOIN dbo.ProductUnits pu ON pu.ProductId = p.ProductId
WHERE p.SKU = 'DEMO-CHICKEN-001'
  AND pu.IsBaseUnit = 1;

SELECT @BeefLotId = LotId
FROM dbo.InventoryLots
WHERE LotCode = 'LOT-DEMO-BEEF-001'
  AND ProductId = @BeefProductId;

SELECT @ChickenLotId = LotId
FROM dbo.InventoryLots
WHERE LotCode = 'LOT-DEMO-CHICKEN-001'
  AND ProductId = @ChickenProductId;

IF @EmployeeId IS NULL
   OR @RetailCustomerTypeId IS NULL
   OR @CurrencyId IS NULL
   OR @SourceWarehouseId IS NULL
   OR @DestinationWarehouseId IS NULL
   OR @SourceBinId IS NULL
   OR @DestinationBinId IS NULL
   OR @BeefProductId IS NULL
   OR @ChickenProductId IS NULL
   OR @BeefUnitId IS NULL
   OR @ChickenUnitId IS NULL
   OR @BeefLotId IS NULL
   OR @ChickenLotId IS NULL
BEGIN
    ;THROW 51001, N'Required demo lookup data is missing; no data was seeded.', 1;
END;

IF @SourceWarehouseId = @DestinationWarehouseId
   OR @SourceBinId = @DestinationBinId
   OR (SELECT COUNT(*) FROM dbo.Warehouses) <> 5
   OR (SELECT COUNT(*)
       FROM dbo.Warehouses w
       JOIN dbo.Employees e
         ON e.EmployeeCode = CONCAT('DEMO-EMP-', RIGHT(w.WarehouseCode, 3))
       WHERE w.WarehouseCode LIKE 'DEMO-WH-%') <> 5
BEGIN
    ;THROW 51002, N'Demo warehouse, bin, or operator lookups do not match the audited state.', 1;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.SystemSettings
        WHERE SettingKey = 'Demo.Seed.Additional.Version'
    )
    BEGIN
        IF (SELECT COUNT(*) FROM dbo.Customers) <> 1
           OR (SELECT COUNT(*) FROM dbo.CustomerAddresses) <> 1
           OR (SELECT COUNT(*) FROM dbo.SalesOrders) <> 1
           OR (SELECT COUNT(*) FROM dbo.SalesOrderDetails) <> 2
           OR (SELECT COUNT(*) FROM dbo.Payments) <> 1
           OR (SELECT COUNT(*) FROM dbo.PaymentDetails) <> 1
           OR (SELECT COUNT(*) FROM dbo.Deliveries) <> 1
           OR (SELECT COUNT(*) FROM dbo.DeliveryDetails) <> 1
           OR (SELECT COUNT(*) FROM dbo.CustomerReturns) <> 1
           OR (SELECT COUNT(*) FROM dbo.CustomerReturnDetails) <> 1
           OR (SELECT COUNT(*) FROM dbo.StockAdjustments) <> 1
           OR (SELECT COUNT(*) FROM dbo.StockAdjustmentDetails) <> 1
           OR (SELECT COUNT(*) FROM dbo.StockTransfers) <> 1
           OR (SELECT COUNT(*) FROM dbo.StockTransferDetails) <> 1
           OR (SELECT COUNT(*) FROM dbo.StockReservations) <> 1
           OR (SELECT COUNT(*) FROM dbo.SystemSettings) <> 3
           OR (SELECT COUNT(*) FROM dbo.WarehouseTemperatureLogs) <> 5
           OR (SELECT COUNT(*) FROM dbo.AuditLogs) <> 12
        BEGIN
            ;THROW 51003, N'The additional demo seed marker exists but expected rows are incomplete or changed.', 1;
        END;

        IF NOT EXISTS
           (SELECT 1 FROM dbo.SystemSettings
            WHERE SettingKey = 'Demo.Seed.Additional.Version'
              AND SettingValue = N'1')
           OR NOT EXISTS
           (SELECT 1 FROM dbo.SalesOrders
            WHERE SalesOrderNumber = 'DEMO-SO-001'
              AND Status = 'PartiallyDelivered')
           OR NOT EXISTS
           (SELECT 1 FROM dbo.CustomerReturns
            WHERE ReturnNumber = 'DEMO-RET-001'
              AND Status = 'Pending')
           OR NOT EXISTS
           (SELECT 1 FROM dbo.StockAdjustmentDetails d
            JOIN dbo.StockAdjustments a ON a.AdjustmentId = d.AdjustmentId
            WHERE a.AdjustmentNumber = 'DEMO-ADJ-001'
              AND d.DifferenceQuantity = 2.000)
           OR NOT EXISTS
           (SELECT 1 FROM dbo.StockTransfers
            WHERE TransferNumber = 'DEMO-TRF-001'
              AND Status = 'Completed')
           OR NOT EXISTS
           (SELECT 1 FROM dbo.StockReservations r
            WHERE r.LotId = @ChickenLotId
              AND r.BinId = @SourceBinId
              AND r.ReservedQuantity = 5.000
              AND r.Status = 'Active')
           OR NOT EXISTS
           (SELECT 1 FROM dbo.InventoryBalances
            WHERE LotId = @BeefLotId
              AND BinId = @SourceBinId
              AND QuantityOnHand = 72.000
              AND QuantityReserved = 0.000)
           OR NOT EXISTS
           (SELECT 1 FROM dbo.InventoryBalances
            WHERE LotId = @BeefLotId
              AND BinId = @DestinationBinId
              AND QuantityOnHand = 5.000
              AND QuantityReserved = 0.000)
           OR NOT EXISTS
           (SELECT 1 FROM dbo.InventoryBalances
            WHERE LotId = @ChickenLotId
              AND BinId = @SourceBinId
              AND QuantityOnHand = 95.000
              AND QuantityReserved = 5.000)
           OR (SELECT SUM(QuantityOnHand)
               FROM dbo.InventoryBalances
               WHERE LotId = @BeefLotId) <> 77.000
           OR (SELECT COUNT(*)
               FROM dbo.InventoryTransactions
               WHERE LotId = @BeefLotId) <> 5
           OR (SELECT SUM(QuantityChange)
               FROM dbo.InventoryTransactions
               WHERE LotId = @BeefLotId) <> 77.000
           OR (SELECT SUM(ReservedQuantity)
               FROM dbo.StockReservations
               WHERE LotId = @ChickenLotId
                 AND BinId = @SourceBinId
                 AND Status = 'Active') <> 5.000
        BEGIN
            ;THROW 51004, N'The additional demo seed marker exists but inventory or workflow verification failed.', 1;
        END;

        COMMIT TRANSACTION;
        PRINT 'Additional demo data is already present and verified; no rows were changed.';
        RETURN;
    END;

    IF EXISTS (SELECT 1 FROM dbo.Customers)
       OR EXISTS (SELECT 1 FROM dbo.CustomerAddresses)
       OR EXISTS (SELECT 1 FROM dbo.SalesOrders)
       OR EXISTS (SELECT 1 FROM dbo.SalesOrderDetails)
       OR EXISTS (SELECT 1 FROM dbo.Payments)
       OR EXISTS (SELECT 1 FROM dbo.PaymentDetails)
       OR EXISTS (SELECT 1 FROM dbo.Deliveries)
       OR EXISTS (SELECT 1 FROM dbo.DeliveryDetails)
       OR EXISTS (SELECT 1 FROM dbo.CustomerReturns)
       OR EXISTS (SELECT 1 FROM dbo.CustomerReturnDetails)
       OR EXISTS (SELECT 1 FROM dbo.StockAdjustments)
       OR EXISTS (SELECT 1 FROM dbo.StockAdjustmentDetails)
       OR EXISTS (SELECT 1 FROM dbo.StockTransfers)
       OR EXISTS (SELECT 1 FROM dbo.StockTransferDetails)
       OR EXISTS (SELECT 1 FROM dbo.StockReservations)
       OR EXISTS (SELECT 1 FROM dbo.SystemSettings)
       OR EXISTS (SELECT 1 FROM dbo.WarehouseTemperatureLogs)
       OR EXISTS (SELECT 1 FROM dbo.AuditLogs)
    BEGIN
        ;THROW 51005, N'Target tables are not empty and the seed marker is absent; stopping without changes.', 1;
    END;

    IF (SELECT COUNT(*) FROM dbo.InventoryBalances WHERE LotId = @BeefLotId) <> 1
       OR (SELECT COUNT(*) FROM dbo.InventoryBalances WHERE LotId = @ChickenLotId) <> 1
       OR NOT EXISTS
          (SELECT 1 FROM dbo.InventoryBalances
           WHERE LotId = @BeefLotId
             AND BinId = @SourceBinId
             AND QuantityOnHand = 85.000
             AND QuantityReserved = 0.000)
       OR NOT EXISTS
          (SELECT 1 FROM dbo.InventoryBalances
           WHERE LotId = @ChickenLotId
             AND BinId = @SourceBinId
             AND QuantityOnHand = 95.000
             AND QuantityReserved = 0.000)
       OR EXISTS
          (SELECT 1 FROM dbo.InventoryBalances
           WHERE LotId = @BeefLotId
             AND BinId = @DestinationBinId)
       OR (SELECT COUNT(*) FROM dbo.InventoryTransactions WHERE LotId = @BeefLotId) <> 1
       OR NOT EXISTS
          (SELECT 1 FROM dbo.InventoryTransactions
           WHERE LotId = @BeefLotId
             AND WarehouseId = @SourceWarehouseId
             AND BinId = @SourceBinId
             AND TransactionType = 'Receipt'
             AND QuantityChange = 85.000)
       OR (SELECT COUNT(*) FROM dbo.InventoryTransactions WHERE LotId = @ChickenLotId) <> 1
       OR NOT EXISTS
          (SELECT 1 FROM dbo.InventoryTransactions
           WHERE LotId = @ChickenLotId
             AND WarehouseId = @SourceWarehouseId
             AND BinId = @SourceBinId
             AND TransactionType = 'Receipt'
             AND QuantityChange = 95.000)
    BEGIN
        ;THROW 51006, N'Inventory preconditions differ from the audited demo state; no data was seeded.', 1;
    END;

    DECLARE @CustomerId int;
    DECLARE @AddressId int;
    DECLARE @SalesOrderId bigint;
    DECLARE @BeefOrderDetailId bigint;
    DECLARE @ChickenOrderDetailId bigint;
    DECLARE @PaymentId bigint;
    DECLARE @DeliveryId bigint;
    DECLARE @CustomerReturnId bigint;
    DECLARE @AdjustmentId bigint;
    DECLARE @TransferId bigint;
    DECLARE @OrderAt datetime2(7) = '2026-11-17T10:00:00';
    DECLARE @AdjustmentAt datetime2(7) = '2026-11-17T09:30:00';
    DECLARE @ReservationAt datetime2(7) = '2026-11-17T10:05:00';
    DECLARE @PlannedDeliveryAt datetime2(7) = '2026-11-17T13:00:00';
    DECLARE @DeliveryAt datetime2(7) = '2026-11-17T14:00:00';
    DECLARE @PaymentAt datetime2(7) = '2026-11-17T14:10:00';
    DECLARE @ReturnAt datetime2(7) = '2026-11-17T15:00:00';
    DECLARE @TransferRequestedAt datetime2(7) = '2026-11-17T15:20:00';
    DECLARE @TransferAt datetime2(7) = '2026-11-17T15:30:00';
    DECLARE @TemperatureAt datetime2(7) = '2026-11-17T16:20:00';

    INSERT INTO dbo.Customers
        (CustomerCode, CustomerName, CustomerTypeId, TaxCode, Phone, Email, IsActive)
    VALUES
        ('DEMO-CUST-001', N'Nhà hàng Demo Thành phố Hồ Chí Minh',
         @RetailCustomerTypeId, 'DEMO-TAX-001', '0000000001',
         'buyer@demo.invalid', 1);
    SET @CustomerId = CONVERT(int, SCOPE_IDENTITY());

    INSERT INTO dbo.CustomerAddresses
        (CustomerId, AddressLine, Province, District, IsDefault)
    VALUES
        (@CustomerId, N'12 Đường Demo', N'TP. Hồ Chí Minh', N'Quận 1', 1);
    SET @AddressId = CONVERT(int, SCOPE_IDENTITY());

    INSERT INTO dbo.SalesOrders
        (SalesOrderNumber, CustomerId, OrderDate, CurrencyId, ExchangeRate,
         Status, CreatedByEmployeeId, Note)
    VALUES
        ('DEMO-SO-001', @CustomerId, @OrderAt, @CurrencyId, 1.000000,
         'PartiallyDelivered', @EmployeeId,
         N'Đơn demo: giao đủ dòng thịt bò; dòng gà giữ một phần hàng đặt trước.');
    SET @SalesOrderId = CONVERT(bigint, SCOPE_IDENTITY());

    INSERT INTO dbo.SalesOrderDetails
        (SalesOrderId, ProductId, Quantity, UnitId, UnitPrice, DiscountAmount)
    VALUES
        (@SalesOrderId, @BeefProductId, 10.000, @BeefUnitId, 250000.0000, 0.0000),
        (@SalesOrderId, @ChickenProductId, 20.000, @ChickenUnitId, 90000.0000, 0.0000);

    SELECT @BeefOrderDetailId = SalesOrderDetailId
    FROM dbo.SalesOrderDetails
    WHERE SalesOrderId = @SalesOrderId
      AND ProductId = @BeefProductId;

    SELECT @ChickenOrderDetailId = SalesOrderDetailId
    FROM dbo.SalesOrderDetails
    WHERE SalesOrderId = @SalesOrderId
      AND ProductId = @ChickenProductId;

    INSERT INTO dbo.Payments
        (PaymentNumber, CustomerId, PaymentDate, CurrencyId, Amount,
         PaymentMethod, Status)
    VALUES
        ('DEMO-PAY-001', @CustomerId, @PaymentAt, @CurrencyId,
         2500000.0000, 'BankTransfer', 'Completed');
    SET @PaymentId = CONVERT(bigint, SCOPE_IDENTITY());

    INSERT INTO dbo.PaymentDetails (PaymentId, SalesOrderId, AppliedAmount)
    VALUES (@PaymentId, @SalesOrderId, 2500000.0000);

    INSERT INTO dbo.Deliveries
        (DeliveryNumber, SalesOrderId, WarehouseId, DeliveryAddressId,
         PlannedAt, ShippedAt, Status)
    VALUES
        ('DEMO-DEL-001', @SalesOrderId, @SourceWarehouseId, @AddressId,
         @PlannedDeliveryAt, @DeliveryAt, 'Delivered');
    SET @DeliveryId = CONVERT(bigint, SCOPE_IDENTITY());

    INSERT INTO dbo.DeliveryDetails
        (DeliveryId, SalesOrderDetailId, LotId, Quantity)
    VALUES
        (@DeliveryId, @BeefOrderDetailId, @BeefLotId, 10.000);

    INSERT INTO dbo.CustomerReturns
        (ReturnNumber, CustomerId, SalesOrderId, ReturnedAt, Reason, Status)
    VALUES
        ('DEMO-RET-001', @CustomerId, @SalesOrderId, @ReturnAt,
         N'Khách yêu cầu trả 2 kg; đang chờ nhận hàng và kiểm tra chất lượng.',
         'Pending');
    SET @CustomerReturnId = CONVERT(bigint, SCOPE_IDENTITY());

    INSERT INTO dbo.CustomerReturnDetails
        (CustomerReturnId, ProductId, LotId, Quantity, ConditionNote)
    VALUES
        (@CustomerReturnId, @BeefProductId, @BeefLotId, 2.000,
         N'Yêu cầu trả hàng đang chờ tiếp nhận; chưa cộng lại tồn kho.');

    INSERT INTO dbo.StockAdjustments
        (AdjustmentNumber, WarehouseId, AdjustmentDate, Reason, Status,
         CreatedByEmployeeId)
    VALUES
        ('DEMO-ADJ-001', @SourceWarehouseId, @AdjustmentAt,
         N'Kiểm kê demo phát hiện thừa 2 kg thịt bò so với số lượng hệ thống.',
         'Approved', @EmployeeId);
    SET @AdjustmentId = CONVERT(bigint, SCOPE_IDENTITY());

    INSERT INTO dbo.StockAdjustmentDetails
        (AdjustmentId, ProductId, LotId, BinId, SystemQuantity, ActualQuantity)
    VALUES
        (@AdjustmentId, @BeefProductId, @BeefLotId, @SourceBinId, 85.000, 87.000);

    UPDATE dbo.InventoryBalances
    SET QuantityOnHand = QuantityOnHand + 2.000,
        UpdatedAt = @AdjustmentAt
    WHERE LotId = @BeefLotId
      AND BinId = @SourceBinId
      AND QuantityOnHand = 85.000
      AND QuantityReserved = 0.000;
    IF @@ROWCOUNT <> 1
    BEGIN
        ;THROW 51007, N'Could not apply the approved inventory adjustment safely.', 1;
    END;

    INSERT INTO dbo.InventoryTransactions
        (LotId, WarehouseId, BinId, TransactionType, QuantityChange,
         ReferenceType, ReferenceId, OccurredAt, CreatedByEmployeeId, Note)
    VALUES
        (@BeefLotId, @SourceWarehouseId, @SourceBinId, 'Adjustment', 2.000,
         'StockAdjustment', @AdjustmentId, @AdjustmentAt, @EmployeeId,
         N'Approved demo adjustment; difference is +2 kg.');

    UPDATE dbo.InventoryBalances
    SET QuantityOnHand = QuantityOnHand - 10.000,
        UpdatedAt = @DeliveryAt
    WHERE LotId = @BeefLotId
      AND BinId = @SourceBinId
      AND QuantityOnHand = 87.000
      AND QuantityReserved = 0.000;
    IF @@ROWCOUNT <> 1
    BEGIN
        ;THROW 51008, N'Could not issue the delivered quantity from inventory safely.', 1;
    END;

    INSERT INTO dbo.InventoryTransactions
        (LotId, WarehouseId, BinId, TransactionType, QuantityChange,
         ReferenceType, ReferenceId, OccurredAt, CreatedByEmployeeId, Note)
    VALUES
        (@BeefLotId, @SourceWarehouseId, @SourceBinId, 'Issue', -10.000,
         'Delivery', @DeliveryId, @DeliveryAt, @EmployeeId,
         N'Issued 10 kg for delivered sales order DEMO-SO-001.');

    INSERT INTO dbo.StockTransfers
        (TransferNumber, SourceWarehouseId, DestinationWarehouseId,
         RequestedAt, Status, RequestedByEmployeeId, Note)
    VALUES
        ('DEMO-TRF-001', @SourceWarehouseId, @DestinationWarehouseId,
         @TransferRequestedAt, 'Completed', @EmployeeId,
         N'Completed demo transfer of 5 kg from HCM cold storage to Da Nang cold storage.');
    SET @TransferId = CONVERT(bigint, SCOPE_IDENTITY());

    INSERT INTO dbo.StockTransferDetails
        (TransferId, ProductId, LotId, SourceBinId, DestinationBinId,
         RequestedQuantity, ReceivedQuantity)
    VALUES
        (@TransferId, @BeefProductId, @BeefLotId, @SourceBinId,
         @DestinationBinId, 5.000, 5.000);

    UPDATE dbo.InventoryBalances
    SET QuantityOnHand = QuantityOnHand - 5.000,
        UpdatedAt = @TransferAt
    WHERE LotId = @BeefLotId
      AND BinId = @SourceBinId
      AND QuantityOnHand = 77.000
      AND QuantityReserved = 0.000;
    IF @@ROWCOUNT <> 1
    BEGIN
        ;THROW 51009, N'Could not transfer stock from the source bin safely.', 1;
    END;

    INSERT INTO dbo.InventoryBalances (LotId, BinId, QuantityOnHand, QuantityReserved, UpdatedAt)
    VALUES (@BeefLotId, @DestinationBinId, 5.000, 0.000, @TransferAt);

    INSERT INTO dbo.InventoryTransactions
        (LotId, WarehouseId, BinId, TransactionType, QuantityChange,
         ReferenceType, ReferenceId, OccurredAt, CreatedByEmployeeId, Note)
    VALUES
        (@BeefLotId, @SourceWarehouseId, @SourceBinId, 'TransferOut', -5.000,
         'StockTransfer', @TransferId, @TransferAt, @EmployeeId,
         N'Transferred 5 kg out to DEMO-WH-DAD.'),
        (@BeefLotId, @DestinationWarehouseId, @DestinationBinId, 'TransferIn', 5.000,
         'StockTransfer', @TransferId, @TransferAt, @EmployeeId,
         N'Received 5 kg from DEMO-WH-HCM.');

    INSERT INTO dbo.StockReservations
        (LotId, BinId, SalesOrderDetailId, ReservedQuantity, Status, CreatedAt)
    VALUES
        (@ChickenLotId, @SourceBinId, @ChickenOrderDetailId, 5.000,
         'Active', @ReservationAt);

    UPDATE dbo.InventoryBalances
    SET QuantityReserved = QuantityReserved + 5.000,
        UpdatedAt = @ReservationAt
    WHERE LotId = @ChickenLotId
      AND BinId = @SourceBinId
      AND QuantityOnHand = 95.000
      AND QuantityReserved = 0.000
      AND QuantityOnHand - QuantityReserved >= 5.000;
    IF @@ROWCOUNT <> 1
    BEGIN
        ;THROW 51010, N'Could not reserve the chicken quantity safely.', 1;
    END;

    INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, Description)
    VALUES
        ('Demo.Seed.Additional.Version', N'1', N'Version marker for the additional WMS demo seed.'),
        ('Demo.Seed.Environment', N'Demo', N'Labels this sample dataset as non-production demo data.'),
        ('Demo.Temperature.LogFrequencyMinutes', N'30', N'Sample interval used by the demo temperature log scenario.');

    INSERT INTO dbo.WarehouseTemperatureLogs
        (WarehouseId, RecordedAt, TemperatureC, RecordedByEmployeeId, Note)
    SELECT w.WarehouseId,
           '2026-11-17T16:20:00',
           v.TemperatureC,
           e.EmployeeId,
           N'Demo ambient receiving-area reading; not a cold-room reading. This schema has no zone link.'
    FROM (VALUES
        ('DEMO-WH-CTH', CONVERT(decimal(5,2), 22.30)),
        ('DEMO-WH-DAD', CONVERT(decimal(5,2), 21.80)),
        ('DEMO-WH-HAN', CONVERT(decimal(5,2), 22.10)),
        ('DEMO-WH-HCM', CONVERT(decimal(5,2), 23.00)),
        ('DEMO-WH-HPH', CONVERT(decimal(5,2), 21.50))
    ) v(WarehouseCode, TemperatureC)
    JOIN dbo.Warehouses w ON w.WarehouseCode = v.WarehouseCode
    JOIN dbo.Employees e
      ON e.EmployeeCode = CONCAT('DEMO-EMP-', RIGHT(w.WarehouseCode, 3));
    IF @@ROWCOUNT <> 5
    BEGIN
        ;THROW 51011, N'Expected five warehouse temperature readings but inserted a different count.', 1;
    END;

    INSERT INTO dbo.AuditLogs
        (EmployeeId, ActionName, EntityName, EntityId, OldValues, NewValues, CreatedAt)
    VALUES
        (@EmployeeId, 'SalesOrder.Created', 'SalesOrder',
         CONVERT(varchar(80), @SalesOrderId), NULL,
         N'{"status":"PartiallyDelivered","customerCode":"DEMO-CUST-001"}', @OrderAt),
        (@EmployeeId, 'StockAdjustment.Approved', 'StockAdjustment',
         CONVERT(varchar(80), @AdjustmentId), NULL,
         N'{"status":"Approved","differenceQuantity":2.000}', @AdjustmentAt),
        (@EmployeeId, 'Delivery.Delivered', 'Delivery',
         CONVERT(varchar(80), @DeliveryId), NULL,
         N'{"status":"Delivered","quantity":10.000}', @DeliveryAt),
        (@EmployeeId, 'Payment.Completed', 'Payment',
         CONVERT(varchar(80), @PaymentId), NULL,
         N'{"status":"Completed","amount":2500000.0000}', @PaymentAt),
        (@EmployeeId, 'CustomerReturn.Requested', 'CustomerReturn',
         CONVERT(varchar(80), @CustomerReturnId), NULL,
         N'{"status":"Pending","quantity":2.000,"restocked":false}', @ReturnAt),
        (@EmployeeId, 'StockTransfer.Completed', 'StockTransfer',
         CONVERT(varchar(80), @TransferId), NULL,
         N'{"status":"Completed","requestedQuantity":5.000,"receivedQuantity":5.000}', @TransferAt),
        (@EmployeeId, 'StockReservation.Activated', 'StockReservation',
         CONVERT(varchar(80),
             (SELECT ReservationId FROM dbo.StockReservations
              WHERE LotId = @ChickenLotId
                AND SalesOrderDetailId = @ChickenOrderDetailId)),
         NULL, N'{"status":"Active","quantity":5.000}', @ReservationAt);

    INSERT INTO dbo.AuditLogs
        (EmployeeId, ActionName, EntityName, EntityId, OldValues, NewValues, CreatedAt)
    SELECT t.RecordedByEmployeeId,
           'WarehouseTemperature.Recorded',
           'WarehouseTemperatureLog',
           CONVERT(varchar(80), t.TemperatureLogId),
           NULL,
           CONCAT(N'{"warehouseCode":"', w.WarehouseCode,
                  N'","temperatureC":', CONVERT(varchar(32), t.TemperatureC),
                  N',"area":"ambient receiving"}'),
           t.RecordedAt
    FROM dbo.WarehouseTemperatureLogs t
    JOIN dbo.Warehouses w ON w.WarehouseId = t.WarehouseId
    WHERE t.RecordedAt = @TemperatureAt
      AND t.Note = N'Demo ambient receiving-area reading; not a cold-room reading. This schema has no zone link.';

    IF (SELECT COUNT(*) FROM dbo.Customers) <> 1
       OR (SELECT COUNT(*) FROM dbo.CustomerAddresses) <> 1
       OR (SELECT COUNT(*) FROM dbo.SalesOrders) <> 1
       OR (SELECT COUNT(*) FROM dbo.SalesOrderDetails) <> 2
       OR (SELECT COUNT(*) FROM dbo.Payments) <> 1
       OR (SELECT COUNT(*) FROM dbo.PaymentDetails) <> 1
       OR (SELECT COUNT(*) FROM dbo.Deliveries) <> 1
       OR (SELECT COUNT(*) FROM dbo.DeliveryDetails) <> 1
       OR (SELECT COUNT(*) FROM dbo.CustomerReturns) <> 1
       OR (SELECT COUNT(*) FROM dbo.CustomerReturnDetails) <> 1
       OR (SELECT COUNT(*) FROM dbo.StockAdjustments) <> 1
       OR (SELECT COUNT(*) FROM dbo.StockAdjustmentDetails) <> 1
       OR (SELECT COUNT(*) FROM dbo.StockTransfers) <> 1
       OR (SELECT COUNT(*) FROM dbo.StockTransferDetails) <> 1
       OR (SELECT COUNT(*) FROM dbo.StockReservations) <> 1
       OR (SELECT COUNT(*) FROM dbo.SystemSettings) <> 3
       OR (SELECT COUNT(*) FROM dbo.WarehouseTemperatureLogs) <> 5
       OR (SELECT COUNT(*) FROM dbo.AuditLogs) <> 12
       OR NOT EXISTS
          (SELECT 1 FROM dbo.InventoryBalances
           WHERE LotId = @BeefLotId
             AND BinId = @SourceBinId
             AND QuantityOnHand = 72.000
             AND QuantityReserved = 0.000)
       OR NOT EXISTS
          (SELECT 1 FROM dbo.InventoryBalances
           WHERE LotId = @BeefLotId
             AND BinId = @DestinationBinId
             AND QuantityOnHand = 5.000
             AND QuantityReserved = 0.000)
       OR NOT EXISTS
          (SELECT 1 FROM dbo.InventoryBalances
           WHERE LotId = @ChickenLotId
             AND BinId = @SourceBinId
             AND QuantityOnHand = 95.000
             AND QuantityReserved = 5.000)
       OR (SELECT SUM(QuantityOnHand)
           FROM dbo.InventoryBalances
           WHERE LotId = @BeefLotId) <> 77.000
    BEGIN
        ;THROW 51012, N'Post-seed row count or inventory consistency verification failed.', 1;
    END;

    COMMIT TRANSACTION;
    PRINT 'Additional demo data seeded and verified successfully in WareHouse.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;
    THROW;
END CATCH;
