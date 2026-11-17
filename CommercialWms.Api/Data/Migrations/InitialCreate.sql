SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Countries] (
    [CountryId] int NOT NULL IDENTITY,
    [CountryCode] char(2) NOT NULL,
    [CountryName] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT (((1))),
    CONSTRAINT [PK_Countries] PRIMARY KEY ([CountryId])
);

CREATE TABLE [Currencies] (
    [CurrencyId] int NOT NULL IDENTITY,
    [CurrencyCode] char(3) NOT NULL,
    [CurrencyName] nvarchar(80) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT (((1))),
    CONSTRAINT [PK_Currencies] PRIMARY KEY ([CurrencyId])
);

CREATE TABLE [CustomerTypes] (
    [CustomerTypeId] int NOT NULL IDENTITY,
    [TypeCode] varchar(30) NOT NULL,
    [TypeName] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_CustomerTypes] PRIMARY KEY ([CustomerTypeId])
);

CREATE TABLE [Employees] (
    [EmployeeId] int NOT NULL IDENTITY,
    [EmployeeCode] varchar(30) NOT NULL,
    [FullName] nvarchar(150) NOT NULL,
    [Email] varchar(150) NULL,
    [Phone] varchar(30) NULL,
    [JobTitle] nvarchar(100) NULL,
    [IsActive] bit NOT NULL DEFAULT (((1))),
    CONSTRAINT [PK_Employees] PRIMARY KEY ([EmployeeId])
);

CREATE TABLE [ProductCategories] (
    [CategoryId] int NOT NULL IDENTITY,
    [ParentCategoryId] int NULL,
    [CategoryCode] varchar(30) NOT NULL,
    [CategoryName] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT (((1))),
    CONSTRAINT [PK_ProductCategories] PRIMARY KEY ([CategoryId]),
    CONSTRAINT [FK_ProductCategories_Parent] FOREIGN KEY ([ParentCategoryId]) REFERENCES [ProductCategories] ([CategoryId])
);

CREATE TABLE [SystemSettings] (
    [SettingId] int NOT NULL IDENTITY,
    [SettingKey] varchar(100) NOT NULL,
    [SettingValue] nvarchar(500) NOT NULL,
    [Description] nvarchar(300) NULL,
    [UpdatedAt] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([SettingId])
);

CREATE TABLE [Units] (
    [UnitId] int NOT NULL IDENTITY,
    [UnitCode] varchar(20) NOT NULL,
    [UnitName] nvarchar(60) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT (((1))),
    CONSTRAINT [PK_Units] PRIMARY KEY ([UnitId])
);

CREATE TABLE [Warehouses] (
    [WarehouseId] int NOT NULL IDENTITY,
    [WarehouseCode] varchar(30) NOT NULL,
    [WarehouseName] nvarchar(150) NOT NULL,
    [Address] nvarchar(300) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT (((1))),
    CONSTRAINT [PK_Warehouses] PRIMARY KEY ([WarehouseId])
);

CREATE TABLE [Brands] (
    [BrandId] int NOT NULL IDENTITY,
    [BrandName] nvarchar(120) NOT NULL,
    [CountryId] int NOT NULL,
    [IsActive] bit NOT NULL DEFAULT (((1))),
    CONSTRAINT [PK_Brands] PRIMARY KEY ([BrandId]),
    CONSTRAINT [FK_Brands_Countries] FOREIGN KEY ([CountryId]) REFERENCES [Countries] ([CountryId])
);

CREATE TABLE [Manufacturers] (
    [ManufacturerId] int NOT NULL IDENTITY,
    [ManufacturerCode] varchar(30) NOT NULL,
    [ManufacturerName] nvarchar(180) NOT NULL,
    [CountryId] int NOT NULL,
    [Address] nvarchar(300) NULL,
    [IsActive] bit NOT NULL DEFAULT (((1))),
    CONSTRAINT [PK_Manufacturers] PRIMARY KEY ([ManufacturerId]),
    CONSTRAINT [FK_Manufacturers_Countries] FOREIGN KEY ([CountryId]) REFERENCES [Countries] ([CountryId])
);

CREATE TABLE [Suppliers] (
    [SupplierId] int NOT NULL IDENTITY,
    [SupplierCode] varchar(30) NOT NULL,
    [SupplierName] nvarchar(180) NOT NULL,
    [CountryId] int NOT NULL,
    [TaxCode] varchar(40) NULL,
    [Email] varchar(150) NULL,
    [Phone] varchar(30) NULL,
    [Address] nvarchar(300) NULL,
    [IsActive] bit NOT NULL DEFAULT (((1))),
    CONSTRAINT [PK_Suppliers] PRIMARY KEY ([SupplierId]),
    CONSTRAINT [FK_Suppliers_Countries] FOREIGN KEY ([CountryId]) REFERENCES [Countries] ([CountryId])
);

CREATE TABLE [ExchangeRates] (
    [ExchangeRateId] bigint NOT NULL IDENTITY,
    [CurrencyId] int NOT NULL,
    [RateDate] date NOT NULL,
    [RateToVND] decimal(18,6) NOT NULL,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK_ExchangeRates] PRIMARY KEY ([ExchangeRateId]),
    CONSTRAINT [FK_ExchangeRates_Currencies] FOREIGN KEY ([CurrencyId]) REFERENCES [Currencies] ([CurrencyId])
);

CREATE TABLE [Customers] (
    [CustomerId] int NOT NULL IDENTITY,
    [CustomerCode] varchar(30) NOT NULL,
    [CustomerName] nvarchar(180) NOT NULL,
    [CustomerTypeId] int NOT NULL,
    [TaxCode] varchar(40) NULL,
    [Phone] varchar(30) NULL,
    [Email] varchar(150) NULL,
    [IsActive] bit NOT NULL DEFAULT (((1))),
    CONSTRAINT [PK_Customers] PRIMARY KEY ([CustomerId]),
    CONSTRAINT [FK_Customers_CustomerTypes] FOREIGN KEY ([CustomerTypeId]) REFERENCES [CustomerTypes] ([CustomerTypeId])
);

CREATE TABLE [AuditLogs] (
    [AuditLogId] bigint NOT NULL IDENTITY,
    [EmployeeId] int NULL,
    [ActionName] varchar(80) NOT NULL,
    [EntityName] varchar(100) NOT NULL,
    [EntityId] varchar(80) NULL,
    [OldValues] nvarchar(max) NULL,
    [NewValues] nvarchar(max) NULL,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([AuditLogId]),
    CONSTRAINT [FK_AuditLogs_Employees] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([EmployeeId])
);

CREATE TABLE [StockAdjustments] (
    [AdjustmentId] bigint NOT NULL IDENTITY,
    [AdjustmentNumber] varchar(40) NOT NULL,
    [WarehouseId] int NOT NULL,
    [AdjustmentDate] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    [Reason] nvarchar(250) NOT NULL,
    [Status] varchar(20) NOT NULL DEFAULT (('Draft')),
    [CreatedByEmployeeId] int NULL,
    CONSTRAINT [PK_StockAdjustments] PRIMARY KEY ([AdjustmentId]),
    CONSTRAINT [CK_StockAdjustments_Status] CHECK ((([Status]='Cancelled' OR [Status]='Rejected' OR [Status]='Approved' OR [Status]='Draft'))),
    CONSTRAINT [FK_StockAdjustments_Employees] FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees] ([EmployeeId]),
    CONSTRAINT [FK_StockAdjustments_Warehouses] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([WarehouseId])
);

CREATE TABLE [StockCounts] (
    [StockCountId] bigint NOT NULL IDENTITY,
    [StockCountNumber] varchar(40) NOT NULL,
    [WarehouseId] int NOT NULL,
    [CountDate] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    [Status] varchar(20) NOT NULL DEFAULT (('Draft')),
    [CreatedByEmployeeId] int NULL,
    CONSTRAINT [PK_StockCounts] PRIMARY KEY ([StockCountId]),
    CONSTRAINT [CK_StockCounts_Status] CHECK ((([Status]='Cancelled' OR [Status]='Completed' OR [Status]='Reviewed' OR [Status]='Counting' OR [Status]='Draft'))),
    CONSTRAINT [FK_StockCounts_Employees] FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees] ([EmployeeId]),
    CONSTRAINT [FK_StockCounts_Warehouses] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([WarehouseId])
);

CREATE TABLE [StockTransfers] (
    [TransferId] bigint NOT NULL IDENTITY,
    [TransferNumber] varchar(40) NOT NULL,
    [SourceWarehouseId] int NOT NULL,
    [DestinationWarehouseId] int NOT NULL,
    [RequestedAt] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    [Status] varchar(20) NOT NULL DEFAULT (('Draft')),
    [RequestedByEmployeeId] int NULL,
    [Note] nvarchar(500) NULL,
    CONSTRAINT [PK_StockTransfers] PRIMARY KEY ([TransferId]),
    CONSTRAINT [CK_StockTransfers_DifferentWarehouses] CHECK ((([SourceWarehouseId]<>[DestinationWarehouseId]))),
    CONSTRAINT [CK_StockTransfers_Status] CHECK ((([Status]='Cancelled' OR [Status]='Completed' OR [Status]='PartiallyReceived' OR [Status]='InTransit' OR [Status]='Approved' OR [Status]='Draft'))),
    CONSTRAINT [FK_StockTransfers_DestinationWarehouse] FOREIGN KEY ([DestinationWarehouseId]) REFERENCES [Warehouses] ([WarehouseId]),
    CONSTRAINT [FK_StockTransfers_Employees] FOREIGN KEY ([RequestedByEmployeeId]) REFERENCES [Employees] ([EmployeeId]),
    CONSTRAINT [FK_StockTransfers_SourceWarehouse] FOREIGN KEY ([SourceWarehouseId]) REFERENCES [Warehouses] ([WarehouseId])
);

CREATE TABLE [WarehouseEmployees] (
    [WarehouseEmployeeId] bigint NOT NULL IDENTITY,
    [WarehouseId] int NOT NULL,
    [EmployeeId] int NOT NULL,
    [AssignedFrom] date NOT NULL,
    [AssignedTo] date NULL,
    CONSTRAINT [PK_WarehouseEmployees] PRIMARY KEY ([WarehouseEmployeeId]),
    CONSTRAINT [CK_WarehouseEmployees_Dates] CHECK ((([AssignedTo] IS NULL OR [AssignedTo]>=[AssignedFrom]))),
    CONSTRAINT [FK_WarehouseEmployees_Warehouses] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([WarehouseId])
);

CREATE TABLE [WarehouseTemperatureLogs] (
    [TemperatureLogId] bigint NOT NULL IDENTITY,
    [WarehouseId] int NOT NULL,
    [RecordedAt] datetime2(7) NOT NULL,
    [TemperatureC] decimal(5,2) NOT NULL,
    [RecordedByEmployeeId] int NULL,
    [Note] nvarchar(300) NULL,
    CONSTRAINT [PK_WarehouseTemperatureLogs] PRIMARY KEY ([TemperatureLogId]),
    CONSTRAINT [FK_WarehouseTemperatureLogs_Warehouses] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([WarehouseId])
);

CREATE TABLE [WarehouseZones] (
    [ZoneId] int NOT NULL IDENTITY,
    [WarehouseId] int NOT NULL,
    [ZoneCode] varchar(30) NOT NULL,
    [ZoneName] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_WarehouseZones] PRIMARY KEY ([ZoneId]),
    CONSTRAINT [FK_WarehouseZones_Warehouses] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([WarehouseId])
);

CREATE TABLE [Products] (
    [ProductId] int NOT NULL IDENTITY,
    [SKU] varchar(40) NOT NULL,
    [ProductName] nvarchar(180) NOT NULL,
    [CategoryId] int NOT NULL,
    [BrandId] int NULL,
    [Barcode] varchar(60) NULL,
    [Description] nvarchar(500) NULL,
    [IsPerishable] bit NOT NULL DEFAULT (((1))),
    [IsActive] bit NOT NULL DEFAULT (((1))),
    [CreatedAt] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK_Products] PRIMARY KEY ([ProductId]),
    CONSTRAINT [FK_Products_Brands] FOREIGN KEY ([BrandId]) REFERENCES [Brands] ([BrandId]),
    CONSTRAINT [FK_Products_Categories] FOREIGN KEY ([CategoryId]) REFERENCES [ProductCategories] ([CategoryId])
);

CREATE TABLE [ManufacturerDocuments] (
    [DocumentId] bigint NOT NULL IDENTITY,
    [ManufacturerId] int NOT NULL,
    [DocumentType] nvarchar(100) NOT NULL,
    [DocumentNumber] varchar(80) NULL,
    [IssueDate] date NULL,
    [ExpiryDate] date NULL,
    [FilePath] nvarchar(500) NULL,
    CONSTRAINT [PK_ManufacturerDocuments] PRIMARY KEY ([DocumentId]),
    CONSTRAINT [CK_ManufacturerDocuments_Dates] CHECK ((([ExpiryDate] IS NULL OR [IssueDate] IS NULL OR [ExpiryDate]>=[IssueDate]))),
    CONSTRAINT [FK_ManufacturerDocuments_Manufacturers] FOREIGN KEY ([ManufacturerId]) REFERENCES [Manufacturers] ([ManufacturerId])
);

CREATE TABLE [FoodSafetyDocuments] (
    [FoodSafetyDocumentId] bigint NOT NULL IDENTITY,
    [SupplierId] int NULL,
    [ManufacturerId] int NULL,
    [LotId] bigint NULL,
    [DocumentType] nvarchar(120) NOT NULL,
    [DocumentNumber] varchar(80) NULL,
    [IssueDate] date NULL,
    [ExpiryDate] date NULL,
    [FilePath] nvarchar(500) NULL,
    CONSTRAINT [PK_FoodSafetyDocuments] PRIMARY KEY ([FoodSafetyDocumentId]),
    CONSTRAINT [CK_FoodSafetyDocuments_Dates] CHECK ((([ExpiryDate] IS NULL OR [IssueDate] IS NULL OR [ExpiryDate]>=[IssueDate]))),
    CONSTRAINT [CK_FoodSafetyDocuments_Owner] CHECK (((((case when [SupplierId] IS NULL then (0) else (1) end+case when [ManufacturerId] IS NULL then (0) else (1) end)+case when [LotId] IS NULL then (0) else (1) end)=(1)))),
    CONSTRAINT [FK_FoodSafetyDocuments_Manufacturers] FOREIGN KEY ([ManufacturerId]) REFERENCES [Manufacturers] ([ManufacturerId]),
    CONSTRAINT [FK_FoodSafetyDocuments_Suppliers] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([SupplierId])
);

CREATE TABLE [PurchaseOrders] (
    [PurchaseOrderId] bigint NOT NULL IDENTITY,
    [PONumber] varchar(40) NOT NULL,
    [SupplierId] int NOT NULL,
    [OrderDate] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    [ExpectedDate] date NULL,
    [CurrencyId] int NOT NULL,
    [ExchangeRate] decimal(18,6) NOT NULL,
    [Status] varchar(20) NOT NULL DEFAULT (('Draft')),
    [CreatedByEmployeeId] int NULL,
    [Note] nvarchar(500) NULL,
    CONSTRAINT [PK_PurchaseOrders] PRIMARY KEY ([PurchaseOrderId]),
    CONSTRAINT [CK_PurchaseOrders_Status] CHECK ((([Status]='Cancelled' OR [Status]='Received' OR [Status]='PartiallyReceived' OR [Status]='Approved' OR [Status]='Submitted' OR [Status]='Draft'))),
    CONSTRAINT [FK_PurchaseOrders_Currencies] FOREIGN KEY ([CurrencyId]) REFERENCES [Currencies] ([CurrencyId]),
    CONSTRAINT [FK_PurchaseOrders_Employees] FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees] ([EmployeeId]),
    CONSTRAINT [FK_PurchaseOrders_Suppliers] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([SupplierId])
);

CREATE TABLE [SupplierContacts] (
    [ContactId] int NOT NULL IDENTITY,
    [SupplierId] int NOT NULL,
    [ContactName] nvarchar(120) NOT NULL,
    [JobTitle] nvarchar(80) NULL,
    [Email] varchar(150) NULL,
    [Phone] varchar(30) NULL,
    CONSTRAINT [PK_SupplierContacts] PRIMARY KEY ([ContactId]),
    CONSTRAINT [FK_SupplierContacts_Suppliers] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([SupplierId])
);

CREATE TABLE [SupplierDocuments] (
    [DocumentId] bigint NOT NULL IDENTITY,
    [SupplierId] int NOT NULL,
    [DocumentType] nvarchar(100) NOT NULL,
    [DocumentNumber] varchar(80) NULL,
    [IssueDate] date NULL,
    [ExpiryDate] date NULL,
    [FilePath] nvarchar(500) NULL,
    CONSTRAINT [PK_SupplierDocuments] PRIMARY KEY ([DocumentId]),
    CONSTRAINT [CK_SupplierDocuments_Dates] CHECK ((([ExpiryDate] IS NULL OR [IssueDate] IS NULL OR [ExpiryDate]>=[IssueDate]))),
    CONSTRAINT [FK_SupplierDocuments_Suppliers] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([SupplierId])
);

CREATE TABLE [CustomerAddresses] (
    [AddressId] int NOT NULL IDENTITY,
    [CustomerId] int NOT NULL,
    [AddressLine] nvarchar(300) NOT NULL,
    [Province] nvarchar(100) NULL,
    [District] nvarchar(100) NULL,
    [IsDefault] bit NOT NULL DEFAULT (((0))),
    CONSTRAINT [PK_CustomerAddresses] PRIMARY KEY ([AddressId]),
    CONSTRAINT [FK_CustomerAddresses_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId])
);

CREATE TABLE [Payments] (
    [PaymentId] bigint NOT NULL IDENTITY,
    [PaymentNumber] varchar(40) NOT NULL,
    [CustomerId] int NOT NULL,
    [PaymentDate] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    [CurrencyId] int NOT NULL,
    [Amount] decimal(18,4) NOT NULL,
    [PaymentMethod] varchar(30) NOT NULL,
    [Status] varchar(20) NOT NULL DEFAULT (('Completed')),
    CONSTRAINT [PK_Payments] PRIMARY KEY ([PaymentId]),
    CONSTRAINT [FK_Payments_Currencies] FOREIGN KEY ([CurrencyId]) REFERENCES [Currencies] ([CurrencyId]),
    CONSTRAINT [FK_Payments_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId])
);

CREATE TABLE [SalesOrders] (
    [SalesOrderId] bigint NOT NULL IDENTITY,
    [SalesOrderNumber] varchar(40) NOT NULL,
    [CustomerId] int NOT NULL,
    [OrderDate] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    [CurrencyId] int NOT NULL,
    [ExchangeRate] decimal(18,6) NOT NULL DEFAULT (((1))),
    [Status] varchar(20) NOT NULL DEFAULT (('Draft')),
    [CreatedByEmployeeId] int NULL,
    [Note] nvarchar(500) NULL,
    CONSTRAINT [PK_SalesOrders] PRIMARY KEY ([SalesOrderId]),
    CONSTRAINT [CK_SalesOrders_Status] CHECK ((([Status]='Cancelled' OR [Status]='Delivered' OR [Status]='PartiallyDelivered' OR [Status]='Confirmed' OR [Status]='Draft'))),
    CONSTRAINT [FK_SalesOrders_Currencies] FOREIGN KEY ([CurrencyId]) REFERENCES [Currencies] ([CurrencyId]),
    CONSTRAINT [FK_SalesOrders_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]),
    CONSTRAINT [FK_SalesOrders_Employees] FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees] ([EmployeeId])
);

CREATE TABLE [WarehouseAisles] (
    [AisleId] int NOT NULL IDENTITY,
    [ZoneId] int NOT NULL,
    [AisleCode] varchar(30) NOT NULL,
    CONSTRAINT [PK_WarehouseAisles] PRIMARY KEY ([AisleId]),
    CONSTRAINT [FK_WarehouseAisles_Zones] FOREIGN KEY ([ZoneId]) REFERENCES [WarehouseZones] ([ZoneId])
);

CREATE TABLE [InventoryLots] (
    [LotId] bigint NOT NULL IDENTITY,
    [LotCode] varchar(60) NOT NULL,
    [ProductId] int NOT NULL,
    [ManufacturerId] int NOT NULL,
    [CountryId] int NOT NULL,
    [ManufactureDate] date NULL,
    [ExpiryDate] date NULL,
    [SupplierLotCode] varchar(60) NULL,
    [CreatedAt] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK_InventoryLots] PRIMARY KEY ([LotId]),
    CONSTRAINT [CK_InventoryLots_Dates] CHECK ((([ExpiryDate] IS NULL OR [ManufactureDate] IS NULL OR [ExpiryDate]>=[ManufactureDate]))),
    CONSTRAINT [FK_InventoryLots_Countries] FOREIGN KEY ([CountryId]) REFERENCES [Countries] ([CountryId]),
    CONSTRAINT [FK_InventoryLots_Manufacturers] FOREIGN KEY ([ManufacturerId]) REFERENCES [Manufacturers] ([ManufacturerId]),
    CONSTRAINT [FK_InventoryLots_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId])
);

CREATE TABLE [ProductStorageRequirements] (
    [RequirementId] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [MinTemperatureC] decimal(5,2) NULL,
    [MaxTemperatureC] decimal(5,2) NULL,
    [StorageNote] nvarchar(300) NULL,
    [EffectiveFrom] date NOT NULL,
    [EffectiveTo] date NULL,
    CONSTRAINT [PK_ProductStorageRequirements] PRIMARY KEY ([RequirementId]),
    CONSTRAINT [CK_ProductStorageRequirements_Dates] CHECK ((([EffectiveTo] IS NULL OR [EffectiveTo]>=[EffectiveFrom]))),
    CONSTRAINT [CK_ProductStorageRequirements_Temp] CHECK ((([MinTemperatureC] IS NULL OR [MaxTemperatureC] IS NULL OR [MinTemperatureC]<=[MaxTemperatureC]))),
    CONSTRAINT [FK_ProductStorageRequirements_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId])
);

CREATE TABLE [ProductUnits] (
    [ProductUnitId] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [UnitId] int NOT NULL,
    [ConversionToBase] decimal(18,6) NOT NULL,
    [IsBaseUnit] bit NOT NULL DEFAULT (((0))),
    CONSTRAINT [PK_ProductUnits] PRIMARY KEY ([ProductUnitId]),
    CONSTRAINT [FK_ProductUnits_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]),
    CONSTRAINT [FK_ProductUnits_Units] FOREIGN KEY ([UnitId]) REFERENCES [Units] ([UnitId])
);

CREATE TABLE [ProductWarehousePolicies] (
    [PolicyId] bigint NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [WarehouseId] int NOT NULL,
    [MinQuantity] decimal(18,3) NOT NULL DEFAULT (((0))),
    [MaxQuantity] decimal(18,3) NULL,
    [SafetyQuantity] decimal(18,3) NOT NULL DEFAULT (((0))),
    CONSTRAINT [PK_ProductWarehousePolicies] PRIMARY KEY ([PolicyId]),
    CONSTRAINT [CK_ProductWarehousePolicies_Max] CHECK ((([MaxQuantity] IS NULL OR [MaxQuantity]>=[MinQuantity]))),
    CONSTRAINT [FK_ProductWarehousePolicies_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId])
);

CREATE TABLE [SupplierProducts] (
    [SupplierProductId] bigint NOT NULL IDENTITY,
    [SupplierId] int NOT NULL,
    [ProductId] int NOT NULL,
    [ManufacturerId] int NOT NULL,
    [SupplierProductCode] varchar(60) NULL,
    [LeadTimeDays] int NOT NULL DEFAULT (((0))),
    [IsActive] bit NOT NULL DEFAULT (((1))),
    CONSTRAINT [PK_SupplierProducts] PRIMARY KEY ([SupplierProductId]),
    CONSTRAINT [FK_SupplierProducts_Manufacturers] FOREIGN KEY ([ManufacturerId]) REFERENCES [Manufacturers] ([ManufacturerId]),
    CONSTRAINT [FK_SupplierProducts_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]),
    CONSTRAINT [FK_SupplierProducts_Suppliers] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([SupplierId])
);

CREATE TABLE [GoodsReceipts] (
    [GoodsReceiptId] bigint NOT NULL IDENTITY,
    [ReceiptNumber] varchar(40) NOT NULL,
    [PurchaseOrderId] bigint NOT NULL,
    [WarehouseId] int NOT NULL,
    [ReceivedAt] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    [ReceivedByEmployeeId] int NULL,
    [Status] varchar(20) NOT NULL DEFAULT (('Draft')),
    [Note] nvarchar(500) NULL,
    CONSTRAINT [PK_GoodsReceipts] PRIMARY KEY ([GoodsReceiptId]),
    CONSTRAINT [CK_GoodsReceipts_Status] CHECK ((([Status]='Cancelled' OR [Status]='Rejected' OR [Status]='PartiallyAccepted' OR [Status]='Accepted' OR [Status]='Checking' OR [Status]='Draft'))),
    CONSTRAINT [FK_GoodsReceipts_Employees] FOREIGN KEY ([ReceivedByEmployeeId]) REFERENCES [Employees] ([EmployeeId]),
    CONSTRAINT [FK_GoodsReceipts_PurchaseOrders] FOREIGN KEY ([PurchaseOrderId]) REFERENCES [PurchaseOrders] ([PurchaseOrderId]),
    CONSTRAINT [FK_GoodsReceipts_Warehouses] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([WarehouseId])
);

CREATE TABLE [PurchaseOrderDetails] (
    [PurchaseOrderDetailId] bigint NOT NULL IDENTITY,
    [PurchaseOrderId] bigint NOT NULL,
    [ProductId] int NOT NULL,
    [OrderedQuantity] decimal(18,3) NOT NULL,
    [UnitId] int NOT NULL,
    [UnitPrice] decimal(18,4) NOT NULL,
    CONSTRAINT [PK_PurchaseOrderDetails] PRIMARY KEY ([PurchaseOrderDetailId]),
    CONSTRAINT [FK_PurchaseOrderDetails_Orders] FOREIGN KEY ([PurchaseOrderId]) REFERENCES [PurchaseOrders] ([PurchaseOrderId]),
    CONSTRAINT [FK_PurchaseOrderDetails_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]),
    CONSTRAINT [FK_PurchaseOrderDetails_Units] FOREIGN KEY ([UnitId]) REFERENCES [Units] ([UnitId])
);

CREATE TABLE [CustomerReturns] (
    [CustomerReturnId] bigint NOT NULL IDENTITY,
    [ReturnNumber] varchar(40) NOT NULL,
    [CustomerId] int NOT NULL,
    [SalesOrderId] bigint NULL,
    [ReturnedAt] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    [Reason] nvarchar(300) NOT NULL,
    [Status] varchar(20) NOT NULL DEFAULT (('Pending')),
    CONSTRAINT [PK_CustomerReturns] PRIMARY KEY ([CustomerReturnId]),
    CONSTRAINT [CK_CustomerReturns_Status] CHECK ((([Status]='Completed' OR [Status]='Rejected' OR [Status]='Accepted' OR [Status]='Pending'))),
    CONSTRAINT [FK_CustomerReturns_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]),
    CONSTRAINT [FK_CustomerReturns_SalesOrders] FOREIGN KEY ([SalesOrderId]) REFERENCES [SalesOrders] ([SalesOrderId])
);

CREATE TABLE [Deliveries] (
    [DeliveryId] bigint NOT NULL IDENTITY,
    [DeliveryNumber] varchar(40) NOT NULL,
    [SalesOrderId] bigint NOT NULL,
    [WarehouseId] int NOT NULL,
    [DeliveryAddressId] int NULL,
    [PlannedAt] datetime2(7) NULL,
    [ShippedAt] datetime2(7) NULL,
    [Status] varchar(20) NOT NULL DEFAULT (('Draft')),
    CONSTRAINT [PK_Deliveries] PRIMARY KEY ([DeliveryId]),
    CONSTRAINT [CK_Deliveries_Status] CHECK ((([Status]='Cancelled' OR [Status]='Failed' OR [Status]='Delivered' OR [Status]='Shipped' OR [Status]='Picking' OR [Status]='Draft'))),
    CONSTRAINT [FK_Deliveries_CustomerAddresses] FOREIGN KEY ([DeliveryAddressId]) REFERENCES [CustomerAddresses] ([AddressId]),
    CONSTRAINT [FK_Deliveries_SalesOrders] FOREIGN KEY ([SalesOrderId]) REFERENCES [SalesOrders] ([SalesOrderId]),
    CONSTRAINT [FK_Deliveries_Warehouses] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([WarehouseId])
);

CREATE TABLE [PaymentDetails] (
    [PaymentDetailId] bigint NOT NULL IDENTITY,
    [PaymentId] bigint NOT NULL,
    [SalesOrderId] bigint NOT NULL,
    [AppliedAmount] decimal(18,4) NOT NULL,
    CONSTRAINT [PK_PaymentDetails] PRIMARY KEY ([PaymentDetailId]),
    CONSTRAINT [FK_PaymentDetails_Payments] FOREIGN KEY ([PaymentId]) REFERENCES [Payments] ([PaymentId]),
    CONSTRAINT [FK_PaymentDetails_SalesOrders] FOREIGN KEY ([SalesOrderId]) REFERENCES [SalesOrders] ([SalesOrderId])
);

CREATE TABLE [SalesOrderDetails] (
    [SalesOrderDetailId] bigint NOT NULL IDENTITY,
    [SalesOrderId] bigint NOT NULL,
    [ProductId] int NOT NULL,
    [Quantity] decimal(18,3) NOT NULL,
    [UnitId] int NOT NULL,
    [UnitPrice] decimal(18,4) NOT NULL,
    [DiscountAmount] decimal(18,4) NOT NULL DEFAULT (((0))),
    CONSTRAINT [PK_SalesOrderDetails] PRIMARY KEY ([SalesOrderDetailId]),
    CONSTRAINT [FK_SalesOrderDetails_Orders] FOREIGN KEY ([SalesOrderId]) REFERENCES [SalesOrders] ([SalesOrderId]),
    CONSTRAINT [FK_SalesOrderDetails_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]),
    CONSTRAINT [FK_SalesOrderDetails_Units] FOREIGN KEY ([UnitId]) REFERENCES [Units] ([UnitId])
);

CREATE TABLE [WarehouseRacks] (
    [RackId] int NOT NULL IDENTITY,
    [AisleId] int NOT NULL,
    [RackCode] varchar(30) NOT NULL,
    CONSTRAINT [PK_WarehouseRacks] PRIMARY KEY ([RackId]),
    CONSTRAINT [FK_WarehouseRacks_Aisles] FOREIGN KEY ([AisleId]) REFERENCES [WarehouseAisles] ([AisleId])
);

CREATE TABLE [QualityInspections] (
    [QualityInspectionId] bigint NOT NULL IDENTITY,
    [InspectionNumber] varchar(40) NOT NULL,
    [GoodsReceiptId] bigint NOT NULL,
    [InspectedAt] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    [InspectorEmployeeId] int NULL,
    [Result] varchar(20) NOT NULL DEFAULT (('Pending')),
    [Note] nvarchar(500) NULL,
    CONSTRAINT [PK_QualityInspections] PRIMARY KEY ([QualityInspectionId]),
    CONSTRAINT [CK_QualityInspections_Result] CHECK ((([Result]='Conditional' OR [Result]='Failed' OR [Result]='Passed' OR [Result]='Pending'))),
    CONSTRAINT [FK_QualityInspections_Employees] FOREIGN KEY ([InspectorEmployeeId]) REFERENCES [Employees] ([EmployeeId]),
    CONSTRAINT [FK_QualityInspections_Receipts] FOREIGN KEY ([GoodsReceiptId]) REFERENCES [GoodsReceipts] ([GoodsReceiptId])
);

CREATE TABLE [GoodsReceiptDetails] (
    [GoodsReceiptDetailId] bigint NOT NULL IDENTITY,
    [GoodsReceiptId] bigint NOT NULL,
    [PurchaseOrderDetailId] bigint NOT NULL,
    [ProductId] int NOT NULL,
    [ReceivedQuantity] decimal(18,3) NOT NULL,
    [AcceptedQuantity] decimal(18,3) NOT NULL DEFAULT (((0))),
    [RejectedQuantity] decimal(18,3) NOT NULL DEFAULT (((0))),
    [UnitId] int NOT NULL,
    CONSTRAINT [PK_GoodsReceiptDetails] PRIMARY KEY ([GoodsReceiptDetailId]),
    CONSTRAINT [CK_GoodsReceiptDetails_Qty] CHECK (((([AcceptedQuantity]+[RejectedQuantity])<=[ReceivedQuantity]))),
    CONSTRAINT [FK_GoodsReceiptDetails_PODetails] FOREIGN KEY ([PurchaseOrderDetailId]) REFERENCES [PurchaseOrderDetails] ([PurchaseOrderDetailId]),
    CONSTRAINT [FK_GoodsReceiptDetails_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]),
    CONSTRAINT [FK_GoodsReceiptDetails_Receipts] FOREIGN KEY ([GoodsReceiptId]) REFERENCES [GoodsReceipts] ([GoodsReceiptId]),
    CONSTRAINT [FK_GoodsReceiptDetails_Units] FOREIGN KEY ([UnitId]) REFERENCES [Units] ([UnitId])
);

CREATE TABLE [PurchasePriceHistories] (
    [PurchasePriceHistoryId] bigint NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [SupplierId] int NOT NULL,
    [PurchaseOrderDetailId] bigint NOT NULL,
    [CurrencyId] int NOT NULL,
    [UnitPrice] decimal(18,4) NOT NULL,
    [RecordedAt] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK_PurchasePriceHistories] PRIMARY KEY ([PurchasePriceHistoryId]),
    CONSTRAINT [FK_PurchasePriceHistories_Currencies] FOREIGN KEY ([CurrencyId]) REFERENCES [Currencies] ([CurrencyId]),
    CONSTRAINT [FK_PurchasePriceHistories_PODetails] FOREIGN KEY ([PurchaseOrderDetailId]) REFERENCES [PurchaseOrderDetails] ([PurchaseOrderDetailId]),
    CONSTRAINT [FK_PurchasePriceHistories_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]),
    CONSTRAINT [FK_PurchasePriceHistories_Suppliers] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([SupplierId])
);

CREATE TABLE [CustomerReturnDetails] (
    [CustomerReturnDetailId] bigint NOT NULL IDENTITY,
    [CustomerReturnId] bigint NOT NULL,
    [ProductId] int NOT NULL,
    [LotId] bigint NULL,
    [Quantity] decimal(18,3) NOT NULL,
    [ConditionNote] nvarchar(250) NULL,
    CONSTRAINT [PK_CustomerReturnDetails] PRIMARY KEY ([CustomerReturnDetailId]),
    CONSTRAINT [FK_CustomerReturnDetails_Lots] FOREIGN KEY ([LotId]) REFERENCES [InventoryLots] ([LotId]),
    CONSTRAINT [FK_CustomerReturnDetails_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]),
    CONSTRAINT [FK_CustomerReturnDetails_Returns] FOREIGN KEY ([CustomerReturnId]) REFERENCES [CustomerReturns] ([CustomerReturnId])
);

CREATE TABLE [DeliveryDetails] (
    [DeliveryDetailId] bigint NOT NULL IDENTITY,
    [DeliveryId] bigint NOT NULL,
    [SalesOrderDetailId] bigint NOT NULL,
    [LotId] bigint NOT NULL,
    [Quantity] decimal(18,3) NOT NULL,
    CONSTRAINT [PK_DeliveryDetails] PRIMARY KEY ([DeliveryDetailId]),
    CONSTRAINT [FK_DeliveryDetails_Deliveries] FOREIGN KEY ([DeliveryId]) REFERENCES [Deliveries] ([DeliveryId]),
    CONSTRAINT [FK_DeliveryDetails_Lots] FOREIGN KEY ([LotId]) REFERENCES [InventoryLots] ([LotId]),
    CONSTRAINT [FK_DeliveryDetails_SalesOrderDetails] FOREIGN KEY ([SalesOrderDetailId]) REFERENCES [SalesOrderDetails] ([SalesOrderDetailId])
);

CREATE TABLE [WarehouseShelves] (
    [ShelfId] int NOT NULL IDENTITY,
    [RackId] int NOT NULL,
    [ShelfCode] varchar(30) NOT NULL,
    CONSTRAINT [PK_WarehouseShelves] PRIMARY KEY ([ShelfId]),
    CONSTRAINT [FK_WarehouseShelves_Racks] FOREIGN KEY ([RackId]) REFERENCES [WarehouseRacks] ([RackId])
);

CREATE TABLE [QualityInspectionDetails] (
    [QualityInspectionDetailId] bigint NOT NULL IDENTITY,
    [QualityInspectionId] bigint NOT NULL,
    [GoodsReceiptDetailId] bigint NOT NULL,
    [CheckName] nvarchar(120) NOT NULL,
    [Result] varchar(20) NOT NULL,
    [Note] nvarchar(300) NULL,
    CONSTRAINT [PK_QualityInspectionDetails] PRIMARY KEY ([QualityInspectionDetailId]),
    CONSTRAINT [CK_QualityInspectionDetails_Result] CHECK ((([Result]='NotApplicable' OR [Result]='Pending' OR [Result]='Fail' OR [Result]='Pass'))),
    CONSTRAINT [FK_QualityInspectionDetails_Inspections] FOREIGN KEY ([QualityInspectionId]) REFERENCES [QualityInspections] ([QualityInspectionId]),
    CONSTRAINT [FK_QualityInspectionDetails_ReceiptDetails] FOREIGN KEY ([GoodsReceiptDetailId]) REFERENCES [GoodsReceiptDetails] ([GoodsReceiptDetailId])
);

CREATE TABLE [WarehouseBins] (
    [BinId] int NOT NULL IDENTITY,
    [ShelfId] int NOT NULL,
    [BinCode] varchar(30) NOT NULL,
    [Capacity] decimal(18,3) NULL,
    [IsActive] bit NOT NULL DEFAULT (((1))),
    CONSTRAINT [PK_WarehouseBins] PRIMARY KEY ([BinId]),
    CONSTRAINT [FK_WarehouseBins_Shelves] FOREIGN KEY ([ShelfId]) REFERENCES [WarehouseShelves] ([ShelfId])
);

CREATE TABLE [InventoryBalances] (
    [BalanceId] bigint NOT NULL IDENTITY,
    [LotId] bigint NOT NULL,
    [BinId] int NOT NULL,
    [QuantityOnHand] decimal(18,3) NOT NULL DEFAULT (((0))),
    [QuantityReserved] decimal(18,3) NOT NULL DEFAULT (((0))),
    [UpdatedAt] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK_InventoryBalances] PRIMARY KEY ([BalanceId]),
    CONSTRAINT [CK_InventoryBalances_Reserved] CHECK ((([QuantityReserved]<=[QuantityOnHand]))),
    CONSTRAINT [FK_InventoryBalances_Bins] FOREIGN KEY ([BinId]) REFERENCES [WarehouseBins] ([BinId]),
    CONSTRAINT [FK_InventoryBalances_Lots] FOREIGN KEY ([LotId]) REFERENCES [InventoryLots] ([LotId])
);

CREATE TABLE [InventoryTransactions] (
    [TransactionId] bigint NOT NULL IDENTITY,
    [LotId] bigint NOT NULL,
    [WarehouseId] int NOT NULL,
    [BinId] int NULL,
    [TransactionType] varchar(30) NOT NULL,
    [QuantityChange] decimal(18,3) NOT NULL,
    [ReferenceType] varchar(30) NULL,
    [ReferenceId] bigint NULL,
    [OccurredAt] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    [CreatedByEmployeeId] int NULL,
    [Note] nvarchar(300) NULL,
    CONSTRAINT [PK_InventoryTransactions] PRIMARY KEY ([TransactionId]),
    CONSTRAINT [CK_InventoryTransactions_Type] CHECK ((([TransactionType]='Return' OR [TransactionType]='Adjustment' OR [TransactionType]='TransferIn' OR [TransactionType]='TransferOut' OR [TransactionType]='Issue' OR [TransactionType]='Receipt'))),
    CONSTRAINT [FK_InventoryTransactions_Bins] FOREIGN KEY ([BinId]) REFERENCES [WarehouseBins] ([BinId]),
    CONSTRAINT [FK_InventoryTransactions_Employees] FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employees] ([EmployeeId]),
    CONSTRAINT [FK_InventoryTransactions_Lots] FOREIGN KEY ([LotId]) REFERENCES [InventoryLots] ([LotId]),
    CONSTRAINT [FK_InventoryTransactions_Warehouses] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([WarehouseId])
);

CREATE TABLE [StockAdjustmentDetails] (
    [AdjustmentDetailId] bigint NOT NULL IDENTITY,
    [AdjustmentId] bigint NOT NULL,
    [ProductId] int NOT NULL,
    [LotId] bigint NOT NULL,
    [BinId] int NOT NULL,
    [SystemQuantity] decimal(18,3) NOT NULL,
    [ActualQuantity] decimal(18,3) NOT NULL,
    [DifferenceQuantity] AS ([ActualQuantity]-[SystemQuantity]) PERSISTED,
    CONSTRAINT [PK_StockAdjustmentDetails] PRIMARY KEY ([AdjustmentDetailId]),
    CONSTRAINT [FK_StockAdjustmentDetails_Adjustments] FOREIGN KEY ([AdjustmentId]) REFERENCES [StockAdjustments] ([AdjustmentId]),
    CONSTRAINT [FK_StockAdjustmentDetails_Bins] FOREIGN KEY ([BinId]) REFERENCES [WarehouseBins] ([BinId]),
    CONSTRAINT [FK_StockAdjustmentDetails_Lots] FOREIGN KEY ([LotId]) REFERENCES [InventoryLots] ([LotId]),
    CONSTRAINT [FK_StockAdjustmentDetails_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId])
);

CREATE TABLE [StockCountDetails] (
    [StockCountDetailId] bigint NOT NULL IDENTITY,
    [StockCountId] bigint NOT NULL,
    [LotId] bigint NOT NULL,
    [BinId] int NOT NULL,
    [SystemQuantity] decimal(18,3) NOT NULL,
    [ActualQuantity] decimal(18,3) NULL,
    [DifferenceQuantity] AS ([ActualQuantity]-[SystemQuantity]) PERSISTED,
    CONSTRAINT [PK_StockCountDetails] PRIMARY KEY ([StockCountDetailId]),
    CONSTRAINT [FK_StockCountDetails_Bins] FOREIGN KEY ([BinId]) REFERENCES [WarehouseBins] ([BinId]),
    CONSTRAINT [FK_StockCountDetails_Lots] FOREIGN KEY ([LotId]) REFERENCES [InventoryLots] ([LotId]),
    CONSTRAINT [FK_StockCountDetails_StockCounts] FOREIGN KEY ([StockCountId]) REFERENCES [StockCounts] ([StockCountId])
);

CREATE TABLE [StockReservations] (
    [ReservationId] bigint NOT NULL IDENTITY,
    [LotId] bigint NOT NULL,
    [BinId] int NOT NULL,
    [SalesOrderDetailId] bigint NULL,
    [ReservedQuantity] decimal(18,3) NOT NULL,
    [Status] varchar(20) NOT NULL DEFAULT (('Active')),
    [CreatedAt] datetime2(7) NOT NULL DEFAULT ((sysutcdatetime())),
    CONSTRAINT [PK_StockReservations] PRIMARY KEY ([ReservationId]),
    CONSTRAINT [CK_StockReservations_Status] CHECK ((([Status]='Cancelled' OR [Status]='Fulfilled' OR [Status]='Released' OR [Status]='Active'))),
    CONSTRAINT [FK_StockReservations_Bins] FOREIGN KEY ([BinId]) REFERENCES [WarehouseBins] ([BinId]),
    CONSTRAINT [FK_StockReservations_Lots] FOREIGN KEY ([LotId]) REFERENCES [InventoryLots] ([LotId])
);

CREATE TABLE [StockTransferDetails] (
    [TransferDetailId] bigint NOT NULL IDENTITY,
    [TransferId] bigint NOT NULL,
    [ProductId] int NOT NULL,
    [LotId] bigint NOT NULL,
    [SourceBinId] int NOT NULL,
    [DestinationBinId] int NOT NULL,
    [RequestedQuantity] decimal(18,3) NOT NULL,
    [ReceivedQuantity] decimal(18,3) NOT NULL DEFAULT (((0))),
    CONSTRAINT [PK_StockTransferDetails] PRIMARY KEY ([TransferDetailId]),
    CONSTRAINT [CK_StockTransferDetails_Received] CHECK ((([ReceivedQuantity]<=[RequestedQuantity]))),
    CONSTRAINT [FK_StockTransferDetails_DestinationBins] FOREIGN KEY ([DestinationBinId]) REFERENCES [WarehouseBins] ([BinId]),
    CONSTRAINT [FK_StockTransferDetails_Lots] FOREIGN KEY ([LotId]) REFERENCES [InventoryLots] ([LotId]),
    CONSTRAINT [FK_StockTransferDetails_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]),
    CONSTRAINT [FK_StockTransferDetails_SourceBins] FOREIGN KEY ([SourceBinId]) REFERENCES [WarehouseBins] ([BinId]),
    CONSTRAINT [FK_StockTransferDetails_Transfers] FOREIGN KEY ([TransferId]) REFERENCES [StockTransfers] ([TransferId])
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CountryId', N'CountryCode', N'CountryName', N'IsActive') AND [object_id] = OBJECT_ID(N'[Countries]'))
    SET IDENTITY_INSERT [Countries] ON;
INSERT INTO [Countries] ([CountryId], [CountryCode], [CountryName], [IsActive])
VALUES (1, 'VN', N'Vietnam', CAST(1 AS bit));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CountryId', N'CountryCode', N'CountryName', N'IsActive') AND [object_id] = OBJECT_ID(N'[Countries]'))
    SET IDENTITY_INSERT [Countries] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CurrencyId', N'CurrencyCode', N'CurrencyName', N'IsActive') AND [object_id] = OBJECT_ID(N'[Currencies]'))
    SET IDENTITY_INSERT [Currencies] ON;
INSERT INTO [Currencies] ([CurrencyId], [CurrencyCode], [CurrencyName], [IsActive])
VALUES (1, 'VND', N'Vietnamese Dong', CAST(1 AS bit));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CurrencyId', N'CurrencyCode', N'CurrencyName', N'IsActive') AND [object_id] = OBJECT_ID(N'[Currencies]'))
    SET IDENTITY_INSERT [Currencies] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CustomerTypeId', N'TypeCode', N'TypeName') AND [object_id] = OBJECT_ID(N'[CustomerTypes]'))
    SET IDENTITY_INSERT [CustomerTypes] ON;
INSERT INTO [CustomerTypes] ([CustomerTypeId], [TypeCode], [TypeName])
VALUES (1, 'RETAIL', N'Retail');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CustomerTypeId', N'TypeCode', N'TypeName') AND [object_id] = OBJECT_ID(N'[CustomerTypes]'))
    SET IDENTITY_INSERT [CustomerTypes] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UnitId', N'IsActive', N'UnitCode', N'UnitName') AND [object_id] = OBJECT_ID(N'[Units]'))
    SET IDENTITY_INSERT [Units] ON;
INSERT INTO [Units] ([UnitId], [IsActive], [UnitCode], [UnitName])
VALUES (1, CAST(1 AS bit), 'EA', N'Each');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UnitId', N'IsActive', N'UnitCode', N'UnitName') AND [object_id] = OBJECT_ID(N'[Units]'))
    SET IDENTITY_INSERT [Units] OFF;

CREATE UNIQUE INDEX [IX_Brands_BrandName_CountryId] ON [Brands] ([BrandName], [CountryId]);

CREATE UNIQUE INDEX [IX_Countries_CountryCode] ON [Countries] ([CountryCode]);

CREATE UNIQUE INDEX [IX_Countries_CountryName] ON [Countries] ([CountryName]);

CREATE UNIQUE INDEX [IX_Currencies_CurrencyCode] ON [Currencies] ([CurrencyCode]);

CREATE UNIQUE INDEX [IX_CustomerReturns_ReturnNumber] ON [CustomerReturns] ([ReturnNumber]);

CREATE UNIQUE INDEX [IX_Customers_CustomerCode] ON [Customers] ([CustomerCode]);

CREATE UNIQUE INDEX [IX_CustomerTypes_TypeCode] ON [CustomerTypes] ([TypeCode]);

CREATE UNIQUE INDEX [IX_Deliveries_DeliveryNumber] ON [Deliveries] ([DeliveryNumber]);

CREATE UNIQUE INDEX [IX_Employees_EmployeeCode] ON [Employees] ([EmployeeCode]);

CREATE UNIQUE INDEX [IX_ExchangeRates_CurrencyId_RateDate] ON [ExchangeRates] ([CurrencyId], [RateDate]);

CREATE UNIQUE INDEX [IX_GoodsReceipts_ReceiptNumber] ON [GoodsReceipts] ([ReceiptNumber]);

CREATE INDEX [IX_GoodsReceipts_Warehouse_Date] ON [GoodsReceipts] ([WarehouseId], [ReceivedAt]);

CREATE INDEX [IX_InventoryBalances_BinId] ON [InventoryBalances] ([BinId]);

CREATE UNIQUE INDEX [IX_InventoryBalances_LotId_BinId] ON [InventoryBalances] ([LotId], [BinId]);

CREATE INDEX [IX_InventoryLots_Product_Expiry] ON [InventoryLots] ([ProductId], [ExpiryDate]);

CREATE UNIQUE INDEX [IX_InventoryLots_ProductId_LotCode] ON [InventoryLots] ([ProductId], [LotCode]);

CREATE INDEX [IX_InventoryTransactions_Lot_Date] ON [InventoryTransactions] ([LotId], [OccurredAt]);

CREATE INDEX [IX_Manufacturers_CountryId] ON [Manufacturers] ([CountryId]);

CREATE UNIQUE INDEX [IX_Manufacturers_ManufacturerCode] ON [Manufacturers] ([ManufacturerCode]);

CREATE UNIQUE INDEX [IX_PaymentDetails_PaymentId_SalesOrderId] ON [PaymentDetails] ([PaymentId], [SalesOrderId]);

CREATE UNIQUE INDEX [IX_Payments_PaymentNumber] ON [Payments] ([PaymentNumber]);

CREATE UNIQUE INDEX [IX_ProductCategories_CategoryCode] ON [ProductCategories] ([CategoryCode]);

CREATE UNIQUE INDEX [IX_Products_Barcode] ON [Products] ([Barcode]) WHERE [Barcode] IS NOT NULL;

CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);

CREATE UNIQUE INDEX [IX_Products_SKU] ON [Products] ([SKU]);

CREATE UNIQUE INDEX [IX_ProductUnits_ProductId_UnitId] ON [ProductUnits] ([ProductId], [UnitId]);

CREATE UNIQUE INDEX [IX_ProductWarehousePolicies_ProductId_WarehouseId] ON [ProductWarehousePolicies] ([ProductId], [WarehouseId]);

CREATE UNIQUE INDEX [IX_PurchaseOrders_PONumber] ON [PurchaseOrders] ([PONumber]);

CREATE INDEX [IX_PurchaseOrders_Supplier_Date] ON [PurchaseOrders] ([SupplierId], [OrderDate]);

CREATE UNIQUE INDEX [IX_QualityInspections_InspectionNumber] ON [QualityInspections] ([InspectionNumber]);

CREATE INDEX [IX_SalesOrders_Customer_Date] ON [SalesOrders] ([CustomerId], [OrderDate]);

CREATE UNIQUE INDEX [IX_SalesOrders_SalesOrderNumber] ON [SalesOrders] ([SalesOrderNumber]);

CREATE UNIQUE INDEX [IX_StockAdjustments_AdjustmentNumber] ON [StockAdjustments] ([AdjustmentNumber]);

CREATE UNIQUE INDEX [IX_StockCountDetails_StockCountId_LotId_BinId] ON [StockCountDetails] ([StockCountId], [LotId], [BinId]);

CREATE UNIQUE INDEX [IX_StockCounts_StockCountNumber] ON [StockCounts] ([StockCountNumber]);

CREATE INDEX [IX_StockTransfers_Status_Date] ON [StockTransfers] ([Status], [RequestedAt]);

CREATE UNIQUE INDEX [IX_StockTransfers_TransferNumber] ON [StockTransfers] ([TransferNumber]);

CREATE UNIQUE INDEX [IX_SupplierProducts_SupplierId_ProductId_ManufacturerId] ON [SupplierProducts] ([SupplierId], [ProductId], [ManufacturerId]);

CREATE INDEX [IX_Suppliers_CountryId] ON [Suppliers] ([CountryId]);

CREATE UNIQUE INDEX [IX_Suppliers_SupplierCode] ON [Suppliers] ([SupplierCode]);

CREATE UNIQUE INDEX [IX_SystemSettings_SettingKey] ON [SystemSettings] ([SettingKey]);

CREATE UNIQUE INDEX [IX_Units_UnitCode] ON [Units] ([UnitCode]);

CREATE UNIQUE INDEX [IX_WarehouseAisles_ZoneId_AisleCode] ON [WarehouseAisles] ([ZoneId], [AisleCode]);

CREATE UNIQUE INDEX [IX_WarehouseBins_ShelfId_BinCode] ON [WarehouseBins] ([ShelfId], [BinCode]);

CREATE UNIQUE INDEX [IX_WarehouseEmployees_WarehouseId_EmployeeId_AssignedFrom] ON [WarehouseEmployees] ([WarehouseId], [EmployeeId], [AssignedFrom]);

CREATE UNIQUE INDEX [IX_WarehouseRacks_AisleId_RackCode] ON [WarehouseRacks] ([AisleId], [RackCode]);

CREATE UNIQUE INDEX [IX_Warehouses_WarehouseCode] ON [Warehouses] ([WarehouseCode]);

CREATE UNIQUE INDEX [IX_WarehouseShelves_RackId_ShelfCode] ON [WarehouseShelves] ([RackId], [ShelfCode]);

CREATE UNIQUE INDEX [IX_WarehouseZones_WarehouseId_ZoneCode] ON [WarehouseZones] ([WarehouseId], [ZoneCode]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261116075251_InitialCreate', N'10.0.12');

COMMIT;
GO
