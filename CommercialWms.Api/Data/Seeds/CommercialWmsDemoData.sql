SET XACT_ABORT ON;
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

IF DB_NAME() <> N'WareHouse'
BEGIN
    THROW 51000, 'This demo seed can only run against the WareHouse database.', 1;
END;

DECLARE @DemoDate date = CONVERT(date, '20261117', 112);
DECLARE @DemoDateTime datetime2(7) = CONVERT(datetime2(7), '2026-11-17T08:00:00');

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @CountrySeed TABLE
    (
        CountryCode char(2) NOT NULL PRIMARY KEY,
        CountryName nvarchar(200) NOT NULL
    );

    INSERT INTO @CountrySeed (CountryCode, CountryName)
    VALUES ('VN', N'Vietnam'), ('CN', N'China'), ('KR', N'South Korea'), ('JP', N'Japan');

    IF EXISTS
    (
        SELECT 1
        FROM @CountrySeed AS seed
        JOIN dbo.Countries AS existing ON existing.CountryCode = seed.CountryCode
        WHERE existing.CountryName <> seed.CountryName
    )
    OR EXISTS
    (
        SELECT 1
        FROM @CountrySeed AS seed
        JOIN dbo.Countries AS existing ON existing.CountryName = seed.CountryName
        WHERE existing.CountryCode <> seed.CountryCode
    )
    BEGIN
        THROW 51001, 'A seeded country code or name already exists with different values.', 1;
    END;

    INSERT INTO dbo.Countries (CountryCode, CountryName, IsActive)
    SELECT seed.CountryCode, seed.CountryName, 1
    FROM @CountrySeed AS seed
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Countries AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.CountryCode = seed.CountryCode
    );

    DECLARE @CurrencySeed TABLE
    (
        CurrencyCode char(3) NOT NULL PRIMARY KEY,
        CurrencyName nvarchar(160) NOT NULL,
        RateToVND decimal(18, 6) NOT NULL
    );

    INSERT INTO @CurrencySeed (CurrencyCode, CurrencyName, RateToVND)
    VALUES
        ('VND', N'Vietnamese Dong', 1.000000),
        ('CNY', N'Chinese Yuan', 3600.000000),
        ('KRW', N'South Korean Won', 18.000000),
        ('JPY', N'Japanese Yen', 170.000000);

    IF EXISTS
    (
        SELECT 1
        FROM @CurrencySeed AS seed
        JOIN dbo.Currencies AS existing ON existing.CurrencyCode = seed.CurrencyCode
        WHERE existing.CurrencyName <> seed.CurrencyName
    )
    BEGIN
        THROW 51002, 'A seeded currency code already exists with a different name.', 1;
    END;

    INSERT INTO dbo.Currencies (CurrencyCode, CurrencyName, IsActive)
    SELECT seed.CurrencyCode, seed.CurrencyName, 1
    FROM @CurrencySeed AS seed
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Currencies AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.CurrencyCode = seed.CurrencyCode
    );

    DECLARE @UnitSeed TABLE
    (
        UnitCode varchar(20) NOT NULL PRIMARY KEY,
        UnitName nvarchar(120) NOT NULL
    );

    INSERT INTO @UnitSeed (UnitCode, UnitName)
    VALUES ('EA', N'Each'), ('KG', N'Kilogram'), ('PACK', N'Pack'), ('BOX', N'Box'), ('BAG', N'Bag');

    IF EXISTS
    (
        SELECT 1
        FROM @UnitSeed AS seed
        JOIN dbo.Units AS existing ON existing.UnitCode = seed.UnitCode
        WHERE existing.UnitName <> seed.UnitName
    )
    BEGIN
        THROW 51003, 'A seeded unit code already exists with a different name.', 1;
    END;

    INSERT INTO dbo.Units (UnitCode, UnitName, IsActive)
    SELECT seed.UnitCode, seed.UnitName, 1
    FROM @UnitSeed AS seed
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Units AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.UnitCode = seed.UnitCode
    );

    DECLARE @CategorySeed TABLE
    (
        CategoryCode varchar(30) NOT NULL PRIMARY KEY,
        CategoryName nvarchar(200) NOT NULL
    );

    INSERT INTO @CategorySeed (CategoryCode, CategoryName)
    VALUES
        ('FROZEN_FOOD', N'Thực phẩm đông lạnh'),
        ('SEAFOOD', N'Hải sản'),
        ('CANDY_SNACK', N'Bánh kẹo và đồ ăn vặt'),
        ('KOREAN_FOOD', N'Thực phẩm Hàn Quốc'),
        ('JAPANESE_FOOD', N'Thực phẩm Nhật Bản');

    IF EXISTS
    (
        SELECT 1
        FROM @CategorySeed AS seed
        JOIN dbo.ProductCategories AS existing ON existing.CategoryCode = seed.CategoryCode
        WHERE existing.CategoryName <> seed.CategoryName OR existing.IsActive <> 1
    )
    BEGIN
        THROW 51004, 'A seeded product category already exists with different values.', 1;
    END;

    INSERT INTO dbo.ProductCategories (CategoryCode, CategoryName, IsActive)
    SELECT seed.CategoryCode, seed.CategoryName, 1
    FROM @CategorySeed AS seed
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.ProductCategories AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.CategoryCode = seed.CategoryCode
    );

    INSERT INTO dbo.ExchangeRates (CurrencyId, RateDate, RateToVND)
    SELECT currency.CurrencyId, @DemoDate, seed.RateToVND
    FROM @CurrencySeed AS seed
    JOIN dbo.Currencies AS currency ON currency.CurrencyCode = seed.CurrencyCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.ExchangeRates AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.CurrencyId = currency.CurrencyId
          AND existing.RateDate = @DemoDate
    );

    IF EXISTS
    (
        SELECT 1
        FROM @CurrencySeed AS seed
        JOIN dbo.Currencies AS currency ON currency.CurrencyCode = seed.CurrencyCode
        JOIN dbo.ExchangeRates AS existing
          ON existing.CurrencyId = currency.CurrencyId
         AND existing.RateDate = @DemoDate
        WHERE existing.RateToVND <> seed.RateToVND
    )
    BEGIN
        THROW 51013, 'A seeded exchange rate already exists with a different value.', 1;
    END;

    DECLARE @SupplierSeed TABLE
    (
        SupplierCode varchar(30) NOT NULL PRIMARY KEY,
        SupplierName nvarchar(360) NOT NULL,
        CountryCode char(2) NOT NULL,
        TaxCode varchar(40) NOT NULL,
        Email varchar(150) NOT NULL,
        Phone varchar(30) NOT NULL,
        Address nvarchar(600) NOT NULL,
        ManufacturerCode varchar(30) NOT NULL,
        ManufacturerName nvarchar(360) NOT NULL,
        BrandName nvarchar(240) NOT NULL,
        CurrencyCode char(3) NOT NULL,
        WarehouseCode varchar(30) NOT NULL
    );

    INSERT INTO @SupplierSeed
    (
        SupplierCode, SupplierName, CountryCode, TaxCode, Email, Phone, Address,
        ManufacturerCode, ManufacturerName, BrandName, CurrencyCode, WarehouseCode
    )
    VALUES
        ('DEMO_SUP_VN_FROZEN', N'Demo Mekong Frozen Foods', 'VN', 'DEMO-TAX-001', 'frozen@example.invalid', '+84-000-000-001', N'Khu công nghiệp Tân Bình, TP. Hồ Chí Minh', 'DEMO_MFR_VN_FROZEN', N'Demo Mekong Frozen Foods Plant', N'Mekong Frozen', 'VND', 'DEMO-WH-HCM'),
        ('DEMO_SUP_VN_SEAFOOD', N'Demo Mekong Seafood', 'VN', 'DEMO-TAX-002', 'seafood@example.invalid', '+84-000-000-002', N'Khu công nghiệp Thọ Quang, Đà Nẵng', 'DEMO_MFR_VN_SEAFOOD', N'Demo Mekong Seafood Processing', N'Mekong Seafood', 'VND', 'DEMO-WH-DAD'),
        ('DEMO_SUP_CN_GROCERY', N'Demo Golden Dragon Foods', 'CN', 'DEMO-TAX-003', 'grocery@example.invalid', '+86-000-000-003', N'Khu công nghiệp chế biến thực phẩm, Quảng Châu', 'DEMO_MFR_CN_GROCERY', N'Demo Golden Dragon Food Works', N'Golden Dragon', 'CNY', 'DEMO-WH-HAN'),
        ('DEMO_SUP_KR_FOOD', N'Demo Seoul Kitchen Foods', 'KR', 'DEMO-TAX-004', 'kfood@example.invalid', '+82-000-000-004', N'Khu công nghiệp thực phẩm, Busan', 'DEMO_MFR_KR_FOOD', N'Demo Seoul Kitchen Plant', N'Seoul Kitchen', 'KRW', 'DEMO-WH-HPH'),
        ('DEMO_SUP_JP_FOOD', N'Demo Tokyo Select Foods', 'JP', 'DEMO-TAX-005', 'jfood@example.invalid', '+81-000-000-005', N'Khu công nghiệp thực phẩm, Osaka', 'DEMO_MFR_JP_FOOD', N'Demo Tokyo Select Processing', N'Tokyo Select', 'JPY', 'DEMO-WH-CTH');

    IF EXISTS
    (
        SELECT 1
        FROM @SupplierSeed AS seed
        JOIN dbo.Suppliers AS existing ON existing.SupplierCode = seed.SupplierCode
        JOIN dbo.Countries AS country ON country.CountryId = existing.CountryId
        WHERE existing.SupplierName <> seed.SupplierName
           OR country.CountryCode <> seed.CountryCode
    )
    BEGIN
        THROW 51005, 'A seeded supplier code already exists with different values.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM @SupplierSeed AS seed
        JOIN dbo.Manufacturers AS existing ON existing.ManufacturerCode = seed.ManufacturerCode
        JOIN dbo.Countries AS country ON country.CountryId = existing.CountryId
        WHERE existing.ManufacturerName <> seed.ManufacturerName
           OR country.CountryCode <> seed.CountryCode
    )
    BEGIN
        THROW 51006, 'A seeded manufacturer code already exists with different values.', 1;
    END;

    INSERT INTO dbo.Brands (BrandName, CountryId, IsActive)
    SELECT DISTINCT seed.BrandName, country.CountryId, 1
    FROM @SupplierSeed AS seed
    JOIN dbo.Countries AS country ON country.CountryCode = seed.CountryCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Brands AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.BrandName = seed.BrandName
          AND existing.CountryId = country.CountryId
    );

    INSERT INTO dbo.Manufacturers (ManufacturerCode, ManufacturerName, CountryId, Address, IsActive)
    SELECT seed.ManufacturerCode, seed.ManufacturerName, country.CountryId,
           seed.Address, 1
    FROM @SupplierSeed AS seed
    JOIN dbo.Countries AS country ON country.CountryCode = seed.CountryCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Manufacturers AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.ManufacturerCode = seed.ManufacturerCode
    );

    INSERT INTO dbo.Suppliers (SupplierCode, SupplierName, CountryId, TaxCode, Email, Phone, Address, IsActive)
    SELECT seed.SupplierCode, seed.SupplierName, country.CountryId, seed.TaxCode,
           seed.Email, seed.Phone, seed.Address, 1
    FROM @SupplierSeed AS seed
    JOIN dbo.Countries AS country ON country.CountryCode = seed.CountryCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Suppliers AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.SupplierCode = seed.SupplierCode
    );

    DECLARE @EmployeeSeed TABLE
    (
        EmployeeCode varchar(30) NOT NULL PRIMARY KEY,
        FullName nvarchar(300) NOT NULL,
        Email varchar(150) NOT NULL,
        Phone varchar(30) NOT NULL,
        JobTitle nvarchar(200) NOT NULL
    );

    INSERT INTO @EmployeeSeed (EmployeeCode, FullName, Email, Phone, JobTitle)
    VALUES
        ('DEMO-EMP-HCM', N'Demo Warehouse Operator HCM', 'operator.hcm@example.invalid', '+84-000-001-001', N'Warehouse Operator'),
        ('DEMO-EMP-DAD', N'Demo Warehouse Operator Da Nang', 'operator.dad@example.invalid', '+84-000-001-002', N'Warehouse Operator'),
        ('DEMO-EMP-HAN', N'Demo Warehouse Operator Hanoi', 'operator.han@example.invalid', '+84-000-001-003', N'Warehouse Operator'),
        ('DEMO-EMP-HPH', N'Demo Warehouse Operator Hai Phong', 'operator.hph@example.invalid', '+84-000-001-004', N'Warehouse Operator'),
        ('DEMO-EMP-CTH', N'Demo Warehouse Operator Can Tho', 'operator.cth@example.invalid', '+84-000-001-005', N'Warehouse Operator');

    IF EXISTS
    (
        SELECT 1
        FROM @EmployeeSeed AS seed
        JOIN dbo.Employees AS existing ON existing.EmployeeCode = seed.EmployeeCode
        WHERE existing.FullName <> seed.FullName
    )
    BEGIN
        THROW 51007, 'A seeded employee code already exists with a different name.', 1;
    END;

    INSERT INTO dbo.Employees (EmployeeCode, FullName, Email, Phone, JobTitle, IsActive)
    SELECT seed.EmployeeCode, seed.FullName, seed.Email, seed.Phone, seed.JobTitle, 1
    FROM @EmployeeSeed AS seed
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Employees AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.EmployeeCode = seed.EmployeeCode
    );

    DECLARE @WarehouseSeed TABLE
    (
        SequenceNo int NOT NULL PRIMARY KEY,
        WarehouseCode varchar(30) NOT NULL UNIQUE,
        WarehouseName nvarchar(300) NOT NULL,
        Address nvarchar(600) NOT NULL,
        EmployeeCode varchar(30) NOT NULL
    );

    INSERT INTO @WarehouseSeed (SequenceNo, WarehouseCode, WarehouseName, Address, EmployeeCode)
    VALUES
        (1, 'DEMO-WH-HCM', N'Kho Demo Thành phố Hồ Chí Minh', N'Khu công nghiệp Tân Bình, TP. Hồ Chí Minh', 'DEMO-EMP-HCM'),
        (2, 'DEMO-WH-DAD', N'Kho Demo Đà Nẵng', N'Khu công nghiệp Thọ Quang, Đà Nẵng', 'DEMO-EMP-DAD'),
        (3, 'DEMO-WH-HAN', N'Kho Demo Hà Nội', N'Khu công nghiệp Thăng Long, Hà Nội', 'DEMO-EMP-HAN'),
        (4, 'DEMO-WH-HPH', N'Kho Demo Hải Phòng', N'Khu công nghiệp Đình Vũ, Hải Phòng', 'DEMO-EMP-HPH'),
        (5, 'DEMO-WH-CTH', N'Kho Demo Cần Thơ', N'Khu công nghiệp Trà Nóc, Cần Thơ', 'DEMO-EMP-CTH');

    IF EXISTS
    (
        SELECT 1
        FROM @WarehouseSeed AS seed
        JOIN dbo.Warehouses AS existing ON existing.WarehouseCode = seed.WarehouseCode
        WHERE existing.WarehouseName <> seed.WarehouseName OR existing.Address <> seed.Address
    )
    BEGIN
        THROW 51008, 'A seeded warehouse code already exists with different values.', 1;
    END;

    INSERT INTO dbo.Warehouses (WarehouseCode, WarehouseName, Address, IsActive)
    SELECT seed.WarehouseCode, seed.WarehouseName, seed.Address, 1
    FROM @WarehouseSeed AS seed
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Warehouses AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.WarehouseCode = seed.WarehouseCode
    );

    INSERT INTO dbo.WarehouseEmployees (WarehouseId, EmployeeId, AssignedFrom)
    SELECT warehouse.WarehouseId, employee.EmployeeId, @DemoDate
    FROM @WarehouseSeed AS seed
    JOIN dbo.Warehouses AS warehouse ON warehouse.WarehouseCode = seed.WarehouseCode
    JOIN dbo.Employees AS employee ON employee.EmployeeCode = seed.EmployeeCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.WarehouseEmployees AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.WarehouseId = warehouse.WarehouseId
          AND existing.EmployeeId = employee.EmployeeId
          AND existing.AssignedFrom = @DemoDate
    );

    INSERT INTO dbo.WarehouseZones (WarehouseId, ZoneCode, ZoneName)
    SELECT warehouse.WarehouseId, zone.ZoneCode, zone.ZoneName
    FROM @WarehouseSeed AS seed
    JOIN dbo.Warehouses AS warehouse ON warehouse.WarehouseCode = seed.WarehouseCode
    CROSS JOIN (VALUES ('COLD', N'Khu bảo quản lạnh'), ('AMBIENT', N'Khu bảo quản khô')) AS zone(ZoneCode, ZoneName)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.WarehouseZones AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.WarehouseId = warehouse.WarehouseId
          AND existing.ZoneCode = zone.ZoneCode
    );

    INSERT INTO dbo.WarehouseAisles (ZoneId, AisleCode)
    SELECT zone.ZoneId, 'A01'
    FROM dbo.WarehouseZones AS zone
    JOIN dbo.Warehouses AS warehouse ON warehouse.WarehouseId = zone.WarehouseId
    JOIN @WarehouseSeed AS warehouseSeed ON warehouseSeed.WarehouseCode = warehouse.WarehouseCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.WarehouseAisles AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.ZoneId = zone.ZoneId AND existing.AisleCode = 'A01'
    );

    INSERT INTO dbo.WarehouseRacks (AisleId, RackCode)
    SELECT aisle.AisleId, 'R01'
    FROM dbo.WarehouseAisles AS aisle
    JOIN dbo.WarehouseZones AS zone ON zone.ZoneId = aisle.ZoneId
    JOIN dbo.Warehouses AS warehouse ON warehouse.WarehouseId = zone.WarehouseId
    JOIN @WarehouseSeed AS warehouseSeed ON warehouseSeed.WarehouseCode = warehouse.WarehouseCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.WarehouseRacks AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.AisleId = aisle.AisleId AND existing.RackCode = 'R01'
    );

    INSERT INTO dbo.WarehouseShelves (RackId, ShelfCode)
    SELECT rack.RackId, 'S01'
    FROM dbo.WarehouseRacks AS rack
    JOIN dbo.WarehouseAisles AS aisle ON aisle.AisleId = rack.AisleId
    JOIN dbo.WarehouseZones AS zone ON zone.ZoneId = aisle.ZoneId
    JOIN dbo.Warehouses AS warehouse ON warehouse.WarehouseId = zone.WarehouseId
    JOIN @WarehouseSeed AS warehouseSeed ON warehouseSeed.WarehouseCode = warehouse.WarehouseCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.WarehouseShelves AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.RackId = rack.RackId AND existing.ShelfCode = 'S01'
    );

    INSERT INTO dbo.WarehouseBins (ShelfId, BinCode, Capacity, IsActive)
    SELECT shelf.ShelfId, 'B01', 5000.000, 1
    FROM dbo.WarehouseShelves AS shelf
    JOIN dbo.WarehouseRacks AS rack ON rack.RackId = shelf.RackId
    JOIN dbo.WarehouseAisles AS aisle ON aisle.AisleId = rack.AisleId
    JOIN dbo.WarehouseZones AS zone ON zone.ZoneId = aisle.ZoneId
    JOIN dbo.Warehouses AS warehouse ON warehouse.WarehouseId = zone.WarehouseId
    JOIN @WarehouseSeed AS warehouseSeed ON warehouseSeed.WarehouseCode = warehouse.WarehouseCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.WarehouseBins AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.ShelfId = shelf.ShelfId AND existing.BinCode = 'B01'
    );

    DECLARE @ProductSeed TABLE
    (
        SKU varchar(40) NOT NULL PRIMARY KEY,
        ProductName nvarchar(360) NOT NULL,
        CategoryCode varchar(30) NOT NULL,
        BrandName nvarchar(240) NULL,
        CountryCode char(2) NOT NULL,
        ManufacturerCode varchar(30) NOT NULL,
        SupplierCode varchar(30) NOT NULL,
        UnitCode varchar(20) NOT NULL,
        Description nvarchar(1000) NOT NULL,
        IsPerishable bit NOT NULL,
        Quantity decimal(18, 3) NOT NULL,
        UnitPrice decimal(18, 4) NOT NULL,
        MinTemperatureC decimal(5, 2) NULL,
        MaxTemperatureC decimal(5, 2) NULL,
        ZoneCode varchar(30) NOT NULL
    );

    INSERT INTO @ProductSeed
    (
        SKU, ProductName, CategoryCode, BrandName, CountryCode, ManufacturerCode,
        SupplierCode, UnitCode, Description, IsPerishable, Quantity, UnitPrice,
        MinTemperatureC, MaxTemperatureC, ZoneCode
    )
    VALUES
        ('FROZEN-PORK-001', N'Frozen Pork', 'FROZEN_FOOD', NULL, 'VN', 'DEMO_MFR_VN_FROZEN', 'DEMO_SUP_VN_FROZEN', 'KG', N'Frozen pork from Vietnam', 1, 120.000, 128000.0000, -18.00, -12.00, 'COLD'),
        ('DEMO-BEEF-001', N'Thịt bò đông lạnh cắt khối', 'FROZEN_FOOD', N'Mekong Frozen', 'VN', 'DEMO_MFR_VN_FROZEN', 'DEMO_SUP_VN_FROZEN', 'KG', N'Thịt bò đông lạnh đóng gói chân không.', 1, 85.000, 219000.0000, -18.00, -12.00, 'COLD'),
        ('DEMO-CHICKEN-001', N'Đùi gà đông lạnh', 'FROZEN_FOOD', N'Mekong Frozen', 'VN', 'DEMO_MFR_VN_FROZEN', 'DEMO_SUP_VN_FROZEN', 'KG', N'Đùi gà đông lạnh đóng gói theo khối lượng.', 1, 95.000, 72000.0000, -18.00, -12.00, 'COLD'),
        ('DEMO-BASA-001', N'Cá basa phi lê', 'SEAFOOD', N'Mekong Seafood', 'VN', 'DEMO_MFR_VN_SEAFOOD', 'DEMO_SUP_VN_SEAFOOD', 'KG', N'Cá basa phi lê cấp đông nhanh.', 1, 110.000, 98000.0000, -25.00, -18.00, 'COLD'),
        ('DEMO-SHRIMP-001', N'Tôm sú đông lạnh', 'SEAFOOD', N'Mekong Seafood', 'VN', 'DEMO_MFR_VN_SEAFOOD', 'DEMO_SUP_VN_SEAFOOD', 'KG', N'Tôm sú sơ chế và cấp đông.', 1, 75.000, 285000.0000, -25.00, -18.00, 'COLD'),
        ('DEMO-SQUID-001', N'Mực khoanh đông lạnh', 'SEAFOOD', N'Mekong Seafood', 'VN', 'DEMO_MFR_VN_SEAFOOD', 'DEMO_SUP_VN_SEAFOOD', 'KG', N'Mực làm sạch, cắt khoanh và cấp đông.', 1, 65.000, 165000.0000, -25.00, -18.00, 'COLD'),
        ('DEMO-CANDY-001', N'Kẹo sữa', 'CANDY_SNACK', N'Golden Dragon', 'CN', 'DEMO_MFR_CN_GROCERY', 'DEMO_SUP_CN_GROCERY', 'PACK', N'Kẹo sữa đóng gói bán lẻ.', 0, 300.000, 24.0000, 10.00, 25.00, 'AMBIENT'),
        ('DEMO-BISCUIT-001', N'Bánh quy mè', 'CANDY_SNACK', N'Golden Dragon', 'CN', 'DEMO_MFR_CN_GROCERY', 'DEMO_SUP_CN_GROCERY', 'BOX', N'Bánh quy mè đóng hộp kín.', 0, 180.000, 32.0000, 10.00, 25.00, 'AMBIENT'),
        ('DEMO-PEANUT-001', N'Đậu phộng cay', 'CANDY_SNACK', N'Golden Dragon', 'CN', 'DEMO_MFR_CN_GROCERY', 'DEMO_SUP_CN_GROCERY', 'PACK', N'Đậu phộng rang gia vị đóng gói.', 0, 220.000, 18.0000, 10.00, 25.00, 'AMBIENT'),
        ('DEMO-KIMBAP-001', N'Kimbap đông lạnh', 'KOREAN_FOOD', N'Seoul Kitchen', 'KR', 'DEMO_MFR_KR_FOOD', 'DEMO_SUP_KR_FOOD', 'PACK', N'Kimbap đóng gói, bảo quản lạnh.', 1, 100.000, 7500.0000, 0.00, 5.00, 'COLD'),
        ('DEMO-TTEOKBOKKI-001', N'Tokbokki', 'KOREAN_FOOD', N'Seoul Kitchen', 'KR', 'DEMO_MFR_KR_FOOD', 'DEMO_SUP_KR_FOOD', 'PACK', N'Bánh gạo tokbokki đóng gói.', 1, 120.000, 9000.0000, 0.00, 5.00, 'COLD'),
        ('DEMO-FISHCAKE-001', N'Chả cá Hàn Quốc', 'KOREAN_FOOD', N'Seoul Kitchen', 'KR', 'DEMO_MFR_KR_FOOD', 'DEMO_SUP_KR_FOOD', 'KG', N'Chả cá bảo quản đông lạnh.', 1, 90.000, 12000.0000, -18.00, -12.00, 'COLD'),
        ('DEMO-KIMCHI-001', N'Kimchi cải thảo', 'KOREAN_FOOD', N'Seoul Kitchen', 'KR', 'DEMO_MFR_KR_FOOD', 'DEMO_SUP_KR_FOOD', 'BOX', N'Kimchi lên men đóng hộp.', 1, 80.000, 15000.0000, 0.00, 5.00, 'COLD'),
        ('DEMO-SALMON-001', N'Cá hồi phi lê', 'SEAFOOD', N'Tokyo Select', 'JP', 'DEMO_MFR_JP_FOOD', 'DEMO_SUP_JP_FOOD', 'KG', N'Cá hồi phi lê cấp đông.', 1, 60.000, 1450.0000, -25.00, -18.00, 'COLD'),
        ('DEMO-SUSHIRICE-001', N'Gạo sushi', 'JAPANESE_FOOD', N'Tokyo Select', 'JP', 'DEMO_MFR_JP_FOOD', 'DEMO_SUP_JP_FOOD', 'BAG', N'Gạo hạt ngắn dùng chế biến sushi.', 0, 140.000, 800.0000, 10.00, 25.00, 'AMBIENT'),
        ('DEMO-MOCHI-001', N'Bánh mochi', 'JAPANESE_FOOD', N'Tokyo Select', 'JP', 'DEMO_MFR_JP_FOOD', 'DEMO_SUP_JP_FOOD', 'BOX', N'Bánh mochi đóng hộp.', 1, 90.000, 900.0000, 0.00, 5.00, 'COLD'),
        ('DEMO-NORI-001', N'Rong biển nori', 'JAPANESE_FOOD', N'Tokyo Select', 'JP', 'DEMO_MFR_JP_FOOD', 'DEMO_SUP_JP_FOOD', 'PACK', N'Rong biển nori sấy khô đóng gói.', 0, 160.000, 600.0000, 10.00, 25.00, 'AMBIENT'),
        ('DEMO-SEAWEED-001', N'Rong biển ăn liền', 'CANDY_SNACK', N'Seoul Kitchen', 'KR', 'DEMO_MFR_KR_FOOD', 'DEMO_SUP_KR_FOOD', 'PACK', N'Rong biển ăn liền đóng gói.', 0, 240.000, 3200.0000, 10.00, 25.00, 'AMBIENT'),
        ('DEMO-DUMPLING-001', N'Há cảo đông lạnh', 'FROZEN_FOOD', N'Golden Dragon', 'CN', 'DEMO_MFR_CN_GROCERY', 'DEMO_SUP_CN_GROCERY', 'BOX', N'Há cảo đông lạnh đóng hộp.', 1, 130.000, 45.0000, -18.00, -12.00, 'COLD'),
        ('DEMO-TEMPURA-001', N'Tôm tempura', 'SEAFOOD', N'Tokyo Select', 'JP', 'DEMO_MFR_JP_FOOD', 'DEMO_SUP_JP_FOOD', 'KG', N'Tôm tempura sơ chế, bảo quản đông lạnh.', 1, 55.000, 2400.0000, -18.00, -12.00, 'COLD');

    IF EXISTS
    (
        SELECT 1
        FROM @ProductSeed AS seed
        JOIN dbo.Products AS existing ON existing.SKU = seed.SKU
        JOIN dbo.ProductCategories AS category ON category.CategoryCode = seed.CategoryCode
        LEFT JOIN dbo.Brands AS brand
          ON brand.BrandName = seed.BrandName
         AND brand.CountryId = (SELECT CountryId FROM dbo.Countries WHERE CountryCode = seed.CountryCode)
        WHERE existing.ProductName <> seed.ProductName
           OR existing.CategoryId <> category.CategoryId
           OR ISNULL(existing.BrandId, -1) <> ISNULL(brand.BrandId, -1)
           OR ISNULL(existing.Description, N'') <> seed.Description
           OR existing.IsPerishable <> seed.IsPerishable
           OR existing.IsActive <> 1
    )
    BEGIN
        THROW 51009, 'A seeded SKU already exists with different product data.', 1;
    END;

    INSERT INTO dbo.Products (SKU, ProductName, CategoryId, BrandId, Description, IsPerishable, IsActive)
    SELECT seed.SKU, seed.ProductName, category.CategoryId, brand.BrandId,
           seed.Description, seed.IsPerishable, 1
    FROM @ProductSeed AS seed
    JOIN dbo.ProductCategories AS category ON category.CategoryCode = seed.CategoryCode
    LEFT JOIN dbo.Brands AS brand
      ON seed.BrandName IS NOT NULL
     AND brand.BrandName = seed.BrandName
     AND brand.CountryId = (SELECT CountryId FROM dbo.Countries WHERE CountryCode = seed.CountryCode)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Products AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.SKU = seed.SKU
    );

    DECLARE @ProductAllocation TABLE
    (
        SKU varchar(40) NOT NULL PRIMARY KEY,
        WarehouseCode varchar(30) NOT NULL,
        ZoneCode varchar(30) NOT NULL
    );

    INSERT INTO @ProductAllocation (SKU, WarehouseCode, ZoneCode)
    SELECT seed.SKU, supplier.WarehouseCode, seed.ZoneCode
    FROM @ProductSeed AS seed
    JOIN @SupplierSeed AS supplier ON supplier.SupplierCode = seed.SupplierCode;

    INSERT INTO dbo.ProductUnits (ProductId, UnitId, ConversionToBase, IsBaseUnit)
    SELECT product.ProductId, unit.UnitId, 1.000000, 1
    FROM @ProductSeed AS seed
    JOIN dbo.Products AS product ON product.SKU = seed.SKU
    JOIN dbo.Units AS unit ON unit.UnitCode = seed.UnitCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.ProductUnits AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.ProductId = product.ProductId AND existing.UnitId = unit.UnitId
    );

    INSERT INTO dbo.ProductStorageRequirements
    (
        ProductId, MinTemperatureC, MaxTemperatureC, StorageNote, EffectiveFrom
    )
    SELECT product.ProductId, seed.MinTemperatureC, seed.MaxTemperatureC,
           N'Demo storage range; verify against the supplier label before operational use.',
           @DemoDate
    FROM @ProductSeed AS seed
    JOIN dbo.Products AS product ON product.SKU = seed.SKU
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.ProductStorageRequirements AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.ProductId = product.ProductId AND existing.EffectiveFrom = @DemoDate
    );

    INSERT INTO dbo.ProductWarehousePolicies
    (
        ProductId, WarehouseId, MinQuantity, MaxQuantity, SafetyQuantity
    )
    SELECT product.ProductId, warehouse.WarehouseId, 5.000, 500.000, 20.000
    FROM @ProductAllocation AS allocation
    JOIN dbo.Products AS product ON product.SKU = allocation.SKU
    JOIN dbo.Warehouses AS warehouse ON warehouse.WarehouseCode = allocation.WarehouseCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.ProductWarehousePolicies AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.ProductId = product.ProductId
          AND existing.WarehouseId = warehouse.WarehouseId
    );

    INSERT INTO dbo.SupplierProducts
    (
        SupplierId, ProductId, ManufacturerId, SupplierProductCode, LeadTimeDays, IsActive
    )
    SELECT supplier.SupplierId, product.ProductId, manufacturer.ManufacturerId,
           CONCAT('DEMO-', seed.SKU),
           CASE supplierSeed.CountryCode WHEN 'VN' THEN 3 WHEN 'CN' THEN 7 WHEN 'KR' THEN 8 ELSE 10 END,
           1
    FROM @ProductSeed AS seed
    JOIN @SupplierSeed AS supplierSeed ON supplierSeed.SupplierCode = seed.SupplierCode
    JOIN dbo.Suppliers AS supplier ON supplier.SupplierCode = seed.SupplierCode
    JOIN dbo.Products AS product ON product.SKU = seed.SKU
    JOIN dbo.Manufacturers AS manufacturer ON manufacturer.ManufacturerCode = seed.ManufacturerCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.SupplierProducts AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.SupplierId = supplier.SupplierId
          AND existing.ProductId = product.ProductId
          AND existing.ManufacturerId = manufacturer.ManufacturerId
    );

    INSERT INTO dbo.SupplierContacts (SupplierId, ContactName, JobTitle, Email, Phone)
    SELECT supplier.SupplierId, CONCAT(N'Liên hệ Demo ', seed.SupplierCode),
           N'Điều phối đơn hàng',
           CONCAT('contact.', REPLACE(seed.SupplierCode, '-', '.'), '@example.invalid'),
           seed.Phone
    FROM @SupplierSeed AS seed
    JOIN dbo.Suppliers AS supplier ON supplier.SupplierCode = seed.SupplierCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.SupplierContacts AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.SupplierId = supplier.SupplierId
          AND existing.Email = CONCAT('contact.', REPLACE(seed.SupplierCode, '-', '.'), '@example.invalid')
    );

    INSERT INTO dbo.SupplierDocuments
    (
        SupplierId, DocumentType, DocumentNumber, IssueDate, ExpiryDate, FilePath
    )
    SELECT supplier.SupplierId, N'Demo business registration',
           CONCAT('DEMO-SUPDOC-', REPLACE(seed.SupplierCode, 'DEMO_SUP_', '')),
           CONVERT(date, '20250101', 112), CONVERT(date, '20271231', 112),
           CONCAT(N'demo://suppliers/', seed.SupplierCode, N'/registration')
    FROM @SupplierSeed AS seed
    JOIN dbo.Suppliers AS supplier ON supplier.SupplierCode = seed.SupplierCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.SupplierDocuments AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.SupplierId = supplier.SupplierId
          AND existing.DocumentNumber = CONCAT('DEMO-SUPDOC-', REPLACE(seed.SupplierCode, 'DEMO_SUP_', ''))
    );

    INSERT INTO dbo.ManufacturerDocuments
    (
        ManufacturerId, DocumentType, DocumentNumber, IssueDate, ExpiryDate, FilePath
    )
    SELECT manufacturer.ManufacturerId, N'Demo quality certificate',
           CONCAT('DEMO-MFRDOC-', REPLACE(seed.ManufacturerCode, 'DEMO_MFR_', '')),
           CONVERT(date, '20250101', 112), CONVERT(date, '20271231', 112),
           CONCAT(N'demo://manufacturers/', seed.ManufacturerCode, N'/quality-certificate')
    FROM @SupplierSeed AS seed
    JOIN dbo.Manufacturers AS manufacturer ON manufacturer.ManufacturerCode = seed.ManufacturerCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.ManufacturerDocuments AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.ManufacturerId = manufacturer.ManufacturerId
          AND existing.DocumentNumber = CONCAT('DEMO-MFRDOC-', REPLACE(seed.ManufacturerCode, 'DEMO_MFR_', ''))
    );

    INSERT INTO dbo.FoodSafetyDocuments
    (
        ManufacturerId, DocumentType, DocumentNumber, IssueDate, ExpiryDate, FilePath
    )
    SELECT manufacturer.ManufacturerId, N'Demo food safety certificate',
           CONCAT('DEMO-FS-', REPLACE(seed.ManufacturerCode, 'DEMO_MFR_', '')),
           CONVERT(date, '20250101', 112), CONVERT(date, '20271231', 112),
           CONCAT(N'demo://manufacturers/', seed.ManufacturerCode, N'/food-safety')
    FROM @SupplierSeed AS seed
    JOIN dbo.Manufacturers AS manufacturer ON manufacturer.ManufacturerCode = seed.ManufacturerCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.FoodSafetyDocuments AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.ManufacturerId = manufacturer.ManufacturerId
          AND existing.DocumentNumber = CONCAT('DEMO-FS-', REPLACE(seed.ManufacturerCode, 'DEMO_MFR_', ''))
    );

    DECLARE @PurchaseOrderSeed TABLE
    (
        SupplierCode varchar(30) NOT NULL PRIMARY KEY,
        PONumber varchar(40) NOT NULL UNIQUE,
        ReceiptNumber varchar(40) NOT NULL UNIQUE,
        InspectionNumber varchar(40) NOT NULL UNIQUE
    );

    INSERT INTO @PurchaseOrderSeed (SupplierCode, PONumber, ReceiptNumber, InspectionNumber)
    VALUES
        ('DEMO_SUP_VN_FROZEN', 'DEMO-PO-VN-FROZEN', 'DEMO-GR-VN-FROZEN', 'DEMO-QI-VN-FROZEN'),
        ('DEMO_SUP_VN_SEAFOOD', 'DEMO-PO-VN-SEAFOOD', 'DEMO-GR-VN-SEAFOOD', 'DEMO-QI-VN-SEAFOOD'),
        ('DEMO_SUP_CN_GROCERY', 'DEMO-PO-CN-GROCERY', 'DEMO-GR-CN-GROCERY', 'DEMO-QI-CN-GROCERY'),
        ('DEMO_SUP_KR_FOOD', 'DEMO-PO-KR-FOOD', 'DEMO-GR-KR-FOOD', 'DEMO-QI-KR-FOOD'),
        ('DEMO_SUP_JP_FOOD', 'DEMO-PO-JP-FOOD', 'DEMO-GR-JP-FOOD', 'DEMO-QI-JP-FOOD');

    IF EXISTS
    (
        SELECT 1
        FROM @PurchaseOrderSeed AS seed
        JOIN dbo.PurchaseOrders AS existing ON existing.PONumber = seed.PONumber
        JOIN dbo.Suppliers AS supplier ON supplier.SupplierId = existing.SupplierId
        WHERE supplier.SupplierCode <> seed.SupplierCode
    )
    BEGIN
        THROW 51010, 'A seeded purchase order number already belongs to a different supplier.', 1;
    END;

    INSERT INTO dbo.PurchaseOrders
    (
        PONumber, SupplierId, OrderDate, ExpectedDate, CurrencyId, ExchangeRate,
        Status, CreatedByEmployeeId, Note
    )
    SELECT orderSeed.PONumber, supplier.SupplierId,
           DATEADD(day, -7, @DemoDateTime), DATEADD(day, -1, @DemoDate),
           currency.CurrencyId, rate.RateToVND, 'Received', employee.EmployeeId,
           N'Demo purchase order; all received quantities are represented in the linked receipt.'
    FROM @PurchaseOrderSeed AS orderSeed
    JOIN @SupplierSeed AS supplierSeed ON supplierSeed.SupplierCode = orderSeed.SupplierCode
    JOIN dbo.Suppliers AS supplier ON supplier.SupplierCode = supplierSeed.SupplierCode
    JOIN dbo.Currencies AS currency ON currency.CurrencyCode = supplierSeed.CurrencyCode
    JOIN @CurrencySeed AS rate ON rate.CurrencyCode = supplierSeed.CurrencyCode
    JOIN dbo.Employees AS employee ON employee.EmployeeCode = 'DEMO-EMP-HCM'
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.PurchaseOrders AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.PONumber = orderSeed.PONumber
    );

    INSERT INTO dbo.PurchaseOrderDetails (PurchaseOrderId, ProductId, OrderedQuantity, UnitId, UnitPrice)
    SELECT purchaseOrder.PurchaseOrderId, product.ProductId, seed.Quantity, unit.UnitId, seed.UnitPrice
    FROM @ProductSeed AS seed
    JOIN dbo.Products AS product ON product.SKU = seed.SKU
    JOIN dbo.Units AS unit ON unit.UnitCode = seed.UnitCode
    JOIN @PurchaseOrderSeed AS orderSeed ON orderSeed.SupplierCode = seed.SupplierCode
    JOIN dbo.PurchaseOrders AS purchaseOrder ON purchaseOrder.PONumber = orderSeed.PONumber
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.PurchaseOrderDetails AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.PurchaseOrderId = purchaseOrder.PurchaseOrderId
          AND existing.ProductId = product.ProductId
    );

    INSERT INTO dbo.GoodsReceipts
    (
        ReceiptNumber, PurchaseOrderId, WarehouseId, ReceivedAt, ReceivedByEmployeeId, Status, Note
    )
    SELECT orderSeed.ReceiptNumber, purchaseOrder.PurchaseOrderId, warehouse.WarehouseId,
           @DemoDateTime, employee.EmployeeId, 'Accepted',
           N'Demo receipt; accepted quantities match the purchase order details.'
    FROM @PurchaseOrderSeed AS orderSeed
    JOIN @SupplierSeed AS supplierSeed ON supplierSeed.SupplierCode = orderSeed.SupplierCode
    JOIN dbo.PurchaseOrders AS purchaseOrder ON purchaseOrder.PONumber = orderSeed.PONumber
    JOIN dbo.Warehouses AS warehouse ON warehouse.WarehouseCode = supplierSeed.WarehouseCode
    JOIN @WarehouseSeed AS warehouseSeed ON warehouseSeed.WarehouseCode = supplierSeed.WarehouseCode
    JOIN dbo.Employees AS employee ON employee.EmployeeCode = warehouseSeed.EmployeeCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.GoodsReceipts AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.ReceiptNumber = orderSeed.ReceiptNumber
    );

    INSERT INTO dbo.GoodsReceiptDetails
    (
        GoodsReceiptId, PurchaseOrderDetailId, ProductId, ReceivedQuantity,
        AcceptedQuantity, RejectedQuantity, UnitId
    )
    SELECT receipt.GoodsReceiptId, orderDetail.PurchaseOrderDetailId,
           product.ProductId, orderDetail.OrderedQuantity, orderDetail.OrderedQuantity,
           0.000, orderDetail.UnitId
    FROM @ProductSeed AS seed
    JOIN dbo.Products AS product ON product.SKU = seed.SKU
    JOIN @PurchaseOrderSeed AS orderSeed ON orderSeed.SupplierCode = seed.SupplierCode
    JOIN dbo.GoodsReceipts AS receipt ON receipt.ReceiptNumber = orderSeed.ReceiptNumber
    JOIN dbo.PurchaseOrders AS purchaseOrder ON purchaseOrder.PurchaseOrderId = receipt.PurchaseOrderId
    JOIN dbo.PurchaseOrderDetails AS orderDetail
      ON orderDetail.PurchaseOrderId = purchaseOrder.PurchaseOrderId
     AND orderDetail.ProductId = product.ProductId
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.GoodsReceiptDetails AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.GoodsReceiptId = receipt.GoodsReceiptId
          AND existing.PurchaseOrderDetailId = orderDetail.PurchaseOrderDetailId
    );

    INSERT INTO dbo.QualityInspections (InspectionNumber, GoodsReceiptId, InspectedAt, InspectorEmployeeId, Result, Note)
    SELECT orderSeed.InspectionNumber, receipt.GoodsReceiptId, @DemoDateTime,
           employee.EmployeeId, 'Passed', N'Demo quality inspection passed.'
    FROM @PurchaseOrderSeed AS orderSeed
    JOIN dbo.GoodsReceipts AS receipt ON receipt.ReceiptNumber = orderSeed.ReceiptNumber
    JOIN @SupplierSeed AS supplierSeed ON supplierSeed.SupplierCode = orderSeed.SupplierCode
    JOIN @WarehouseSeed AS warehouseSeed ON warehouseSeed.WarehouseCode = supplierSeed.WarehouseCode
    JOIN dbo.Employees AS employee ON employee.EmployeeCode = warehouseSeed.EmployeeCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.QualityInspections AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.InspectionNumber = orderSeed.InspectionNumber
    );

    INSERT INTO dbo.QualityInspectionDetails (QualityInspectionId, GoodsReceiptDetailId, CheckName, Result, Note)
    SELECT inspection.QualityInspectionId, receiptDetail.GoodsReceiptDetailId,
           checkSeed.CheckName, 'Pass', checkSeed.Note
    FROM @PurchaseOrderSeed AS orderSeed
    JOIN dbo.GoodsReceipts AS receipt ON receipt.ReceiptNumber = orderSeed.ReceiptNumber
    JOIN dbo.QualityInspections AS inspection ON inspection.GoodsReceiptId = receipt.GoodsReceiptId
    JOIN dbo.GoodsReceiptDetails AS receiptDetail ON receiptDetail.GoodsReceiptId = receipt.GoodsReceiptId
    CROSS JOIN
    (
        VALUES
            (N'Packaging condition', N'Packaging was intact during demo inspection.'),
            (N'Product temperature', N'Temperature was within the demo storage range.')
    ) AS checkSeed(CheckName, Note)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.QualityInspectionDetails AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.QualityInspectionId = inspection.QualityInspectionId
          AND existing.GoodsReceiptDetailId = receiptDetail.GoodsReceiptDetailId
          AND existing.CheckName = checkSeed.CheckName
    );

    INSERT INTO dbo.InventoryLots
    (
        LotCode, ProductId, ManufacturerId, CountryId, ManufactureDate, ExpiryDate,
        SupplierLotCode
    )
    SELECT CONCAT('LOT-', seed.SKU), product.ProductId, manufacturer.ManufacturerId,
           country.CountryId, DATEADD(day, -30, @DemoDate), DATEADD(day, 365, @DemoDate),
           CONCAT('SUP-', seed.SKU)
    FROM @ProductSeed AS seed
    JOIN dbo.Products AS product ON product.SKU = seed.SKU
    JOIN dbo.Manufacturers AS manufacturer ON manufacturer.ManufacturerCode = seed.ManufacturerCode
    JOIN dbo.Countries AS country ON country.CountryCode = seed.CountryCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.InventoryLots AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.ProductId = product.ProductId
          AND existing.LotCode = CONCAT('LOT-', seed.SKU)
    );

    INSERT INTO dbo.InventoryBalances (LotId, BinId, QuantityOnHand, QuantityReserved, UpdatedAt)
    SELECT lot.LotId, bin.BinId, seed.Quantity, 0.000, @DemoDateTime
    FROM @ProductSeed AS seed
    JOIN @ProductAllocation AS allocation ON allocation.SKU = seed.SKU
    JOIN dbo.Products AS product ON product.SKU = seed.SKU
    JOIN dbo.InventoryLots AS lot ON lot.ProductId = product.ProductId
                                  AND lot.LotCode = CONCAT('LOT-', seed.SKU)
    JOIN dbo.Warehouses AS warehouse ON warehouse.WarehouseCode = allocation.WarehouseCode
    JOIN dbo.WarehouseZones AS zone ON zone.WarehouseId = warehouse.WarehouseId
                                   AND zone.ZoneCode = allocation.ZoneCode
    JOIN dbo.WarehouseAisles AS aisle ON aisle.ZoneId = zone.ZoneId AND aisle.AisleCode = 'A01'
    JOIN dbo.WarehouseRacks AS rack ON rack.AisleId = aisle.AisleId AND rack.RackCode = 'R01'
    JOIN dbo.WarehouseShelves AS shelf ON shelf.RackId = rack.RackId AND shelf.ShelfCode = 'S01'
    JOIN dbo.WarehouseBins AS bin ON bin.ShelfId = shelf.ShelfId AND bin.BinCode = 'B01'
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.InventoryBalances AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.LotId = lot.LotId AND existing.BinId = bin.BinId
    );

    INSERT INTO dbo.InventoryTransactions
    (
        LotId, WarehouseId, BinId, TransactionType, QuantityChange,
        ReferenceType, ReferenceId, OccurredAt, CreatedByEmployeeId, Note
    )
    SELECT lot.LotId, warehouse.WarehouseId, bin.BinId, 'Receipt', seed.Quantity,
           'GoodsReceipt', receipt.GoodsReceiptId, @DemoDateTime, employee.EmployeeId,
           N'Demo receipt transaction; quantity equals the linked inventory balance.'
    FROM @ProductSeed AS seed
    JOIN @ProductAllocation AS allocation ON allocation.SKU = seed.SKU
    JOIN dbo.Products AS product ON product.SKU = seed.SKU
    JOIN dbo.InventoryLots AS lot ON lot.ProductId = product.ProductId
                                  AND lot.LotCode = CONCAT('LOT-', seed.SKU)
    JOIN dbo.Warehouses AS warehouse ON warehouse.WarehouseCode = allocation.WarehouseCode
    JOIN dbo.WarehouseZones AS zone ON zone.WarehouseId = warehouse.WarehouseId
                                   AND zone.ZoneCode = allocation.ZoneCode
    JOIN dbo.WarehouseAisles AS aisle ON aisle.ZoneId = zone.ZoneId AND aisle.AisleCode = 'A01'
    JOIN dbo.WarehouseRacks AS rack ON rack.AisleId = aisle.AisleId AND rack.RackCode = 'R01'
    JOIN dbo.WarehouseShelves AS shelf ON shelf.RackId = rack.RackId AND shelf.ShelfCode = 'S01'
    JOIN dbo.WarehouseBins AS bin ON bin.ShelfId = shelf.ShelfId AND bin.BinCode = 'B01'
    JOIN @PurchaseOrderSeed AS orderSeed ON orderSeed.SupplierCode = seed.SupplierCode
    JOIN dbo.GoodsReceipts AS receipt ON receipt.ReceiptNumber = orderSeed.ReceiptNumber
    JOIN @WarehouseSeed AS warehouseSeed ON warehouseSeed.WarehouseCode = allocation.WarehouseCode
    JOIN dbo.Employees AS employee ON employee.EmployeeCode = warehouseSeed.EmployeeCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.InventoryTransactions AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.TransactionType = 'Receipt'
          AND existing.ReferenceType = 'GoodsReceipt'
          AND existing.ReferenceId = receipt.GoodsReceiptId
          AND existing.LotId = lot.LotId
          AND existing.BinId = bin.BinId
    );

    INSERT INTO dbo.PurchasePriceHistories
    (
        ProductId, SupplierId, PurchaseOrderDetailId, CurrencyId, UnitPrice, RecordedAt
    )
    SELECT product.ProductId, supplier.SupplierId, orderDetail.PurchaseOrderDetailId,
           currency.CurrencyId, orderDetail.UnitPrice, @DemoDateTime
    FROM @ProductSeed AS seed
    JOIN dbo.Products AS product ON product.SKU = seed.SKU
    JOIN dbo.Suppliers AS supplier ON supplier.SupplierCode = seed.SupplierCode
    JOIN dbo.PurchaseOrders AS purchaseOrder ON purchaseOrder.SupplierId = supplier.SupplierId
    JOIN dbo.PurchaseOrderDetails AS orderDetail
      ON orderDetail.PurchaseOrderId = purchaseOrder.PurchaseOrderId
     AND orderDetail.ProductId = product.ProductId
    JOIN @SupplierSeed AS supplierSeed ON supplierSeed.SupplierCode = seed.SupplierCode
    JOIN dbo.Currencies AS currency ON currency.CurrencyCode = supplierSeed.CurrencyCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.PurchasePriceHistories AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.PurchaseOrderDetailId = orderDetail.PurchaseOrderDetailId
    );

    INSERT INTO dbo.StockCounts (StockCountNumber, WarehouseId, CountDate, Status, CreatedByEmployeeId)
    SELECT CONCAT('DEMO-SC-', seed.WarehouseCode), warehouse.WarehouseId,
           @DemoDateTime, 'Completed', employee.EmployeeId
    FROM @WarehouseSeed AS seed
    JOIN dbo.Warehouses AS warehouse ON warehouse.WarehouseCode = seed.WarehouseCode
    JOIN dbo.Employees AS employee ON employee.EmployeeCode = seed.EmployeeCode
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.StockCounts AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.StockCountNumber = CONCAT('DEMO-SC-', seed.WarehouseCode)
    );

    INSERT INTO dbo.StockCountDetails (StockCountId, LotId, BinId, SystemQuantity, ActualQuantity)
    SELECT stockCount.StockCountId, balance.LotId, balance.BinId,
           balance.QuantityOnHand, balance.QuantityOnHand
    FROM @ProductAllocation AS allocation
    JOIN dbo.Products AS product ON product.SKU = allocation.SKU
    JOIN dbo.InventoryLots AS lot ON lot.ProductId = product.ProductId
                                  AND lot.LotCode = CONCAT('LOT-', allocation.SKU)
    JOIN dbo.InventoryBalances AS balance ON balance.LotId = lot.LotId
    JOIN dbo.WarehouseBins AS bin ON bin.BinId = balance.BinId
    JOIN dbo.WarehouseShelves AS shelf ON shelf.ShelfId = bin.ShelfId
    JOIN dbo.WarehouseRacks AS rack ON rack.RackId = shelf.RackId
    JOIN dbo.WarehouseAisles AS aisle ON aisle.AisleId = rack.AisleId
    JOIN dbo.WarehouseZones AS zone ON zone.ZoneId = aisle.ZoneId
    JOIN dbo.Warehouses AS warehouse ON warehouse.WarehouseId = zone.WarehouseId
    JOIN dbo.StockCounts AS stockCount
      ON stockCount.WarehouseId = warehouse.WarehouseId
     AND stockCount.StockCountNumber = CONCAT('DEMO-SC-', allocation.WarehouseCode)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.StockCountDetails AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.StockCountId = stockCount.StockCountId
          AND existing.LotId = balance.LotId
          AND existing.BinId = balance.BinId
    );

    IF (SELECT COUNT(*) FROM @ProductSeed AS seed JOIN dbo.Products AS product ON product.SKU = seed.SKU) <> 20
       OR (SELECT COUNT(*) FROM @SupplierSeed AS seed JOIN dbo.Suppliers AS supplier ON supplier.SupplierCode = seed.SupplierCode) <> 5
       OR (SELECT COUNT(*) FROM @SupplierSeed AS seed JOIN dbo.Manufacturers AS manufacturer ON manufacturer.ManufacturerCode = seed.ManufacturerCode) <> 5
       OR (SELECT COUNT(*) FROM @WarehouseSeed AS seed JOIN dbo.Warehouses AS warehouse ON warehouse.WarehouseCode = seed.WarehouseCode) <> 5
       OR (SELECT COUNT(*) FROM @PurchaseOrderSeed AS seed JOIN dbo.PurchaseOrders AS purchaseOrder ON purchaseOrder.PONumber = seed.PONumber) <> 5
       OR (SELECT COUNT(*) FROM @PurchaseOrderSeed AS seed JOIN dbo.GoodsReceipts AS receipt ON receipt.ReceiptNumber = seed.ReceiptNumber) <> 5
       OR (SELECT COUNT(*) FROM @PurchaseOrderSeed AS seed JOIN dbo.QualityInspections AS inspection ON inspection.InspectionNumber = seed.InspectionNumber) <> 5
       OR (SELECT COUNT(*) FROM @ProductSeed AS seed JOIN dbo.Products AS product ON product.SKU = seed.SKU
           JOIN dbo.InventoryLots AS lot ON lot.ProductId = product.ProductId AND lot.LotCode = CONCAT('LOT-', seed.SKU)) <> 20
       OR (SELECT COUNT(*) FROM @ProductSeed AS seed JOIN dbo.Products AS product ON product.SKU = seed.SKU
           JOIN dbo.InventoryLots AS lot ON lot.ProductId = product.ProductId AND lot.LotCode = CONCAT('LOT-', seed.SKU)
           JOIN dbo.InventoryBalances AS balance ON balance.LotId = lot.LotId) <> 20
       OR (SELECT COUNT(*) FROM @ProductSeed AS seed JOIN dbo.Products AS product ON product.SKU = seed.SKU
           JOIN dbo.PurchaseOrderDetails AS detail ON detail.ProductId = product.ProductId
           JOIN dbo.PurchaseOrders AS purchaseOrder ON purchaseOrder.PurchaseOrderId = detail.PurchaseOrderId
           JOIN @PurchaseOrderSeed AS orderSeed ON orderSeed.PONumber = purchaseOrder.PONumber
           WHERE detail.OrderedQuantity = seed.Quantity AND detail.UnitPrice = seed.UnitPrice) <> 20
       OR (SELECT COUNT(*) FROM dbo.QualityInspectionDetails AS detail
           JOIN dbo.QualityInspections AS inspection ON inspection.QualityInspectionId = detail.QualityInspectionId
           JOIN @PurchaseOrderSeed AS orderSeed ON orderSeed.InspectionNumber = inspection.InspectionNumber
           WHERE detail.Result = 'Pass') <> 40
       OR (SELECT COUNT(*) FROM dbo.StockCounts AS stockCount
           JOIN @WarehouseSeed AS warehouseSeed
             ON stockCount.StockCountNumber = CONCAT('DEMO-SC-', warehouseSeed.WarehouseCode)) <> 5
       OR (SELECT COUNT(*) FROM dbo.StockCountDetails AS detail
           JOIN dbo.StockCounts AS stockCount ON stockCount.StockCountId = detail.StockCountId
           JOIN @WarehouseSeed AS warehouseSeed
             ON stockCount.StockCountNumber = CONCAT('DEMO-SC-', warehouseSeed.WarehouseCode)) <> 20
    BEGIN
        THROW 51011, 'Demo seed verification failed; all changes have been rolled back.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM @ProductSeed AS seed
        JOIN dbo.Products AS product ON product.SKU = seed.SKU
        JOIN dbo.InventoryLots AS lot ON lot.ProductId = product.ProductId
                                      AND lot.LotCode = CONCAT('LOT-', seed.SKU)
        JOIN dbo.InventoryBalances AS balance ON balance.LotId = lot.LotId
        WHERE balance.QuantityOnHand <> seed.Quantity
           OR balance.QuantityReserved <> 0
    )
    OR EXISTS
    (
        SELECT 1
        FROM dbo.StockCountDetails AS detail
        JOIN dbo.StockCounts AS stockCount ON stockCount.StockCountId = detail.StockCountId
        JOIN @WarehouseSeed AS warehouseSeed
          ON stockCount.StockCountNumber = CONCAT('DEMO-SC-', warehouseSeed.WarehouseCode)
        WHERE detail.ActualQuantity <> detail.SystemQuantity
           OR detail.DifferenceQuantity <> 0
    )
    BEGIN
        THROW 51012, 'Demo inventory or stock count quantities are inconsistent; all changes have been rolled back.', 1;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
