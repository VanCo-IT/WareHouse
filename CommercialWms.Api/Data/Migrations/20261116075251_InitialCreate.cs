using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommercialWms.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    CountryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryCode = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    CountryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.CountryId);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    CurrencyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyCode = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    CurrencyName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.CurrencyId);
                });

            migrationBuilder.CreateTable(
                name: "CustomerTypes",
                columns: table => new
                {
                    CustomerTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerTypes", x => x.CustomerTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    CategoryCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_ProductCategories_Parent",
                        column: x => x.ParentCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "CategoryId");
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    SettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettingKey = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.SettingId);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    UnitName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.UnitId);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    WarehouseName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.WarehouseId);
                });

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    BrandId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.BrandId);
                    table.ForeignKey(
                        name: "FK_Brands_Countries",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId");
                });

            migrationBuilder.CreateTable(
                name: "Manufacturers",
                columns: table => new
                {
                    ManufacturerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManufacturerCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    ManufacturerName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manufacturers", x => x.ManufacturerId);
                    table.ForeignKey(
                        name: "FK_Manufacturers_Countries",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId");
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    SupplierId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    TaxCode = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    Email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.SupplierId);
                    table.ForeignKey(
                        name: "FK_Suppliers_Countries",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId");
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    ExchangeRateId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    RateDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RateToVND = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.ExchangeRateId);
                    table.ForeignKey(
                        name: "FK_ExchangeRates_Currencies",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "CurrencyId");
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    CustomerTypeId = table.Column<int>(type: "int", nullable: false),
                    TaxCode = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    Phone = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_Customers_CustomerTypes",
                        column: x => x.CustomerTypeId,
                        principalTable: "CustomerTypes",
                        principalColumn: "CustomerTypeId");
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    ActionName = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    EntityName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Employees",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustments",
                columns: table => new
                {
                    AdjustmentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjustmentNumber = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    AdjustmentDate = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    Reason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValueSql: "('Draft')"),
                    CreatedByEmployeeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustments", x => x.AdjustmentId);
                    table.CheckConstraint("CK_StockAdjustments_Status", "(([Status]='Cancelled' OR [Status]='Rejected' OR [Status]='Approved' OR [Status]='Draft'))");
                    table.ForeignKey(
                        name: "FK_StockAdjustments_Employees",
                        column: x => x.CreatedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_StockAdjustments_Warehouses",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "StockCounts",
                columns: table => new
                {
                    StockCountId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockCountNumber = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    CountDate = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValueSql: "('Draft')"),
                    CreatedByEmployeeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockCounts", x => x.StockCountId);
                    table.CheckConstraint("CK_StockCounts_Status", "(([Status]='Cancelled' OR [Status]='Completed' OR [Status]='Reviewed' OR [Status]='Counting' OR [Status]='Draft'))");
                    table.ForeignKey(
                        name: "FK_StockCounts_Employees",
                        column: x => x.CreatedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_StockCounts_Warehouses",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "StockTransfers",
                columns: table => new
                {
                    TransferId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferNumber = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    SourceWarehouseId = table.Column<int>(type: "int", nullable: false),
                    DestinationWarehouseId = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValueSql: "('Draft')"),
                    RequestedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransfers", x => x.TransferId);
                    table.CheckConstraint("CK_StockTransfers_DifferentWarehouses", "(([SourceWarehouseId]<>[DestinationWarehouseId]))");
                    table.CheckConstraint("CK_StockTransfers_Status", "(([Status]='Cancelled' OR [Status]='Completed' OR [Status]='PartiallyReceived' OR [Status]='InTransit' OR [Status]='Approved' OR [Status]='Draft'))");
                    table.ForeignKey(
                        name: "FK_StockTransfers_DestinationWarehouse",
                        column: x => x.DestinationWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                    table.ForeignKey(
                        name: "FK_StockTransfers_Employees",
                        column: x => x.RequestedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_StockTransfers_SourceWarehouse",
                        column: x => x.SourceWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "WarehouseEmployees",
                columns: table => new
                {
                    WarehouseEmployeeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    AssignedFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    AssignedTo = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseEmployees", x => x.WarehouseEmployeeId);
                    table.CheckConstraint("CK_WarehouseEmployees_Dates", "(([AssignedTo] IS NULL OR [AssignedTo]>=[AssignedFrom]))");
                    table.ForeignKey(
                        name: "FK_WarehouseEmployees_Warehouses",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "WarehouseTemperatureLogs",
                columns: table => new
                {
                    TemperatureLogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    TemperatureC = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    RecordedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseTemperatureLogs", x => x.TemperatureLogId);
                    table.ForeignKey(
                        name: "FK_WarehouseTemperatureLogs_Warehouses",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "WarehouseZones",
                columns: table => new
                {
                    ZoneId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    ZoneCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    ZoneName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseZones", x => x.ZoneId);
                    table.ForeignKey(
                        name: "FK_WarehouseZones_Warehouses",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SKU = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: true),
                    Barcode = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPerishable = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Brands",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "BrandId");
                    table.ForeignKey(
                        name: "FK_Products_Categories",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "CategoryId");
                });

            migrationBuilder.CreateTable(
                name: "ManufacturerDocuments",
                columns: table => new
                {
                    DocumentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManufacturerId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentNumber = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturerDocuments", x => x.DocumentId);
                    table.CheckConstraint("CK_ManufacturerDocuments_Dates", "(([ExpiryDate] IS NULL OR [IssueDate] IS NULL OR [ExpiryDate]>=[IssueDate]))");
                    table.ForeignKey(
                        name: "FK_ManufacturerDocuments_Manufacturers",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "ManufacturerId");
                });

            migrationBuilder.CreateTable(
                name: "FoodSafetyDocuments",
                columns: table => new
                {
                    FoodSafetyDocumentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    ManufacturerId = table.Column<int>(type: "int", nullable: true),
                    LotId = table.Column<long>(type: "bigint", nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DocumentNumber = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodSafetyDocuments", x => x.FoodSafetyDocumentId);
                    table.CheckConstraint("CK_FoodSafetyDocuments_Dates", "(([ExpiryDate] IS NULL OR [IssueDate] IS NULL OR [ExpiryDate]>=[IssueDate]))");
                    table.CheckConstraint("CK_FoodSafetyDocuments_Owner", "((((case when [SupplierId] IS NULL then (0) else (1) end+case when [ManufacturerId] IS NULL then (0) else (1) end)+case when [LotId] IS NULL then (0) else (1) end)=(1)))");
                    table.ForeignKey(
                        name: "FK_FoodSafetyDocuments_Manufacturers",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "ManufacturerId");
                    table.ForeignKey(
                        name: "FK_FoodSafetyDocuments_Suppliers",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    PurchaseOrderId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PONumber = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    ExpectedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValueSql: "('Draft')"),
                    CreatedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.PurchaseOrderId);
                    table.CheckConstraint("CK_PurchaseOrders_Status", "(([Status]='Cancelled' OR [Status]='Received' OR [Status]='PartiallyReceived' OR [Status]='Approved' OR [Status]='Submitted' OR [Status]='Draft'))");
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Currencies",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "CurrencyId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Employees",
                        column: x => x.CreatedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Suppliers",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId");
                });

            migrationBuilder.CreateTable(
                name: "SupplierContacts",
                columns: table => new
                {
                    ContactId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierContacts", x => x.ContactId);
                    table.ForeignKey(
                        name: "FK_SupplierContacts_Suppliers",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId");
                });

            migrationBuilder.CreateTable(
                name: "SupplierDocuments",
                columns: table => new
                {
                    DocumentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentNumber = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDocuments", x => x.DocumentId);
                    table.CheckConstraint("CK_SupplierDocuments_Dates", "(([ExpiryDate] IS NULL OR [IssueDate] IS NULL OR [ExpiryDate]>=[IssueDate]))");
                    table.ForeignKey(
                        name: "FK_SupplierDocuments_Suppliers",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId");
                });

            migrationBuilder.CreateTable(
                name: "CustomerAddresses",
                columns: table => new
                {
                    AddressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    AddressLine = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((0))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAddresses", x => x.AddressId);
                    table.ForeignKey(
                        name: "FK_CustomerAddresses_Customers",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentNumber = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PaymentMethod = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValueSql: "('Completed')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Currencies",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "CurrencyId");
                    table.ForeignKey(
                        name: "FK_Payments_Customers",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                });

            migrationBuilder.CreateTable(
                name: "SalesOrders",
                columns: table => new
                {
                    SalesOrderId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderNumber = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false, defaultValueSql: "((1))"),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValueSql: "('Draft')"),
                    CreatedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrders", x => x.SalesOrderId);
                    table.CheckConstraint("CK_SalesOrders_Status", "(([Status]='Cancelled' OR [Status]='Delivered' OR [Status]='PartiallyDelivered' OR [Status]='Confirmed' OR [Status]='Draft'))");
                    table.ForeignKey(
                        name: "FK_SalesOrders_Currencies",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "CurrencyId");
                    table.ForeignKey(
                        name: "FK_SalesOrders_Customers",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_SalesOrders_Employees",
                        column: x => x.CreatedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                });

            migrationBuilder.CreateTable(
                name: "WarehouseAisles",
                columns: table => new
                {
                    AisleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    AisleCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseAisles", x => x.AisleId);
                    table.ForeignKey(
                        name: "FK_WarehouseAisles_Zones",
                        column: x => x.ZoneId,
                        principalTable: "WarehouseZones",
                        principalColumn: "ZoneId");
                });

            migrationBuilder.CreateTable(
                name: "InventoryLots",
                columns: table => new
                {
                    LotId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotCode = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ManufacturerId = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    ManufactureDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SupplierLotCode = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryLots", x => x.LotId);
                    table.CheckConstraint("CK_InventoryLots_Dates", "(([ExpiryDate] IS NULL OR [ManufactureDate] IS NULL OR [ExpiryDate]>=[ManufactureDate]))");
                    table.ForeignKey(
                        name: "FK_InventoryLots_Countries",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId");
                    table.ForeignKey(
                        name: "FK_InventoryLots_Manufacturers",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "ManufacturerId");
                    table.ForeignKey(
                        name: "FK_InventoryLots_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                });

            migrationBuilder.CreateTable(
                name: "ProductStorageRequirements",
                columns: table => new
                {
                    RequirementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    MinTemperatureC = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    MaxTemperatureC = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    StorageNote = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStorageRequirements", x => x.RequirementId);
                    table.CheckConstraint("CK_ProductStorageRequirements_Dates", "(([EffectiveTo] IS NULL OR [EffectiveTo]>=[EffectiveFrom]))");
                    table.CheckConstraint("CK_ProductStorageRequirements_Temp", "(([MinTemperatureC] IS NULL OR [MaxTemperatureC] IS NULL OR [MinTemperatureC]<=[MaxTemperatureC]))");
                    table.ForeignKey(
                        name: "FK_ProductStorageRequirements_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                });

            migrationBuilder.CreateTable(
                name: "ProductUnits",
                columns: table => new
                {
                    ProductUnitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ConversionToBase = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    IsBaseUnit = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((0))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductUnits", x => x.ProductUnitId);
                    table.ForeignKey(
                        name: "FK_ProductUnits_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                    table.ForeignKey(
                        name: "FK_ProductUnits_Units",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "ProductWarehousePolicies",
                columns: table => new
                {
                    PolicyId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    MinQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false, defaultValueSql: "((0))"),
                    MaxQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    SafetyQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false, defaultValueSql: "((0))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductWarehousePolicies", x => x.PolicyId);
                    table.CheckConstraint("CK_ProductWarehousePolicies_Max", "(([MaxQuantity] IS NULL OR [MaxQuantity]>=[MinQuantity]))");
                    table.ForeignKey(
                        name: "FK_ProductWarehousePolicies_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                });

            migrationBuilder.CreateTable(
                name: "SupplierProducts",
                columns: table => new
                {
                    SupplierProductId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ManufacturerId = table.Column<int>(type: "int", nullable: false),
                    SupplierProductCode = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((0))"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierProducts", x => x.SupplierProductId);
                    table.ForeignKey(
                        name: "FK_SupplierProducts_Manufacturers",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "ManufacturerId");
                    table.ForeignKey(
                        name: "FK_SupplierProducts_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                    table.ForeignKey(
                        name: "FK_SupplierProducts_Suppliers",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId");
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceipts",
                columns: table => new
                {
                    GoodsReceiptId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptNumber = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    PurchaseOrderId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    ReceivedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValueSql: "('Draft')"),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceipts", x => x.GoodsReceiptId);
                    table.CheckConstraint("CK_GoodsReceipts_Status", "(([Status]='Cancelled' OR [Status]='Rejected' OR [Status]='PartiallyAccepted' OR [Status]='Accepted' OR [Status]='Checking' OR [Status]='Draft'))");
                    table.ForeignKey(
                        name: "FK_GoodsReceipts_Employees",
                        column: x => x.ReceivedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_GoodsReceipts_PurchaseOrders",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "PurchaseOrderId");
                    table.ForeignKey(
                        name: "FK_GoodsReceipts_Warehouses",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderDetails",
                columns: table => new
                {
                    PurchaseOrderDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    OrderedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderDetails", x => x.PurchaseOrderDetailId);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetails_Orders",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "PurchaseOrderId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetails_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetails_Units",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "CustomerReturns",
                columns: table => new
                {
                    CustomerReturnId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnNumber = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderId = table.Column<long>(type: "bigint", nullable: true),
                    ReturnedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValueSql: "('Pending')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerReturns", x => x.CustomerReturnId);
                    table.CheckConstraint("CK_CustomerReturns_Status", "(([Status]='Completed' OR [Status]='Rejected' OR [Status]='Accepted' OR [Status]='Pending'))");
                    table.ForeignKey(
                        name: "FK_CustomerReturns_Customers",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_CustomerReturns_SalesOrders",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "SalesOrderId");
                });

            migrationBuilder.CreateTable(
                name: "Deliveries",
                columns: table => new
                {
                    DeliveryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryNumber = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    SalesOrderId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    DeliveryAddressId = table.Column<int>(type: "int", nullable: true),
                    PlannedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ShippedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValueSql: "('Draft')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.DeliveryId);
                    table.CheckConstraint("CK_Deliveries_Status", "(([Status]='Cancelled' OR [Status]='Failed' OR [Status]='Delivered' OR [Status]='Shipped' OR [Status]='Picking' OR [Status]='Draft'))");
                    table.ForeignKey(
                        name: "FK_Deliveries_CustomerAddresses",
                        column: x => x.DeliveryAddressId,
                        principalTable: "CustomerAddresses",
                        principalColumn: "AddressId");
                    table.ForeignKey(
                        name: "FK_Deliveries_SalesOrders",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "SalesOrderId");
                    table.ForeignKey(
                        name: "FK_Deliveries_Warehouses",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "PaymentDetails",
                columns: table => new
                {
                    PaymentDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<long>(type: "bigint", nullable: false),
                    SalesOrderId = table.Column<long>(type: "bigint", nullable: false),
                    AppliedAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentDetails", x => x.PaymentDetailId);
                    table.ForeignKey(
                        name: "FK_PaymentDetails_Payments",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId");
                    table.ForeignKey(
                        name: "FK_PaymentDetails_SalesOrders",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "SalesOrderId");
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderDetails",
                columns: table => new
                {
                    SalesOrderDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValueSql: "((0))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderDetails", x => x.SalesOrderDetailId);
                    table.ForeignKey(
                        name: "FK_SalesOrderDetails_Orders",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "SalesOrderId");
                    table.ForeignKey(
                        name: "FK_SalesOrderDetails_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                    table.ForeignKey(
                        name: "FK_SalesOrderDetails_Units",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "WarehouseRacks",
                columns: table => new
                {
                    RackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AisleId = table.Column<int>(type: "int", nullable: false),
                    RackCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseRacks", x => x.RackId);
                    table.ForeignKey(
                        name: "FK_WarehouseRacks_Aisles",
                        column: x => x.AisleId,
                        principalTable: "WarehouseAisles",
                        principalColumn: "AisleId");
                });

            migrationBuilder.CreateTable(
                name: "QualityInspections",
                columns: table => new
                {
                    QualityInspectionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionNumber = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    GoodsReceiptId = table.Column<long>(type: "bigint", nullable: false),
                    InspectedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    InspectorEmployeeId = table.Column<int>(type: "int", nullable: true),
                    Result = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValueSql: "('Pending')"),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityInspections", x => x.QualityInspectionId);
                    table.CheckConstraint("CK_QualityInspections_Result", "(([Result]='Conditional' OR [Result]='Failed' OR [Result]='Passed' OR [Result]='Pending'))");
                    table.ForeignKey(
                        name: "FK_QualityInspections_Employees",
                        column: x => x.InspectorEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_QualityInspections_Receipts",
                        column: x => x.GoodsReceiptId,
                        principalTable: "GoodsReceipts",
                        principalColumn: "GoodsReceiptId");
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceiptDetails",
                columns: table => new
                {
                    GoodsReceiptDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GoodsReceiptId = table.Column<long>(type: "bigint", nullable: false),
                    PurchaseOrderDetailId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    AcceptedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false, defaultValueSql: "((0))"),
                    RejectedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false, defaultValueSql: "((0))"),
                    UnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceiptDetails", x => x.GoodsReceiptDetailId);
                    table.CheckConstraint("CK_GoodsReceiptDetails_Qty", "((([AcceptedQuantity]+[RejectedQuantity])<=[ReceivedQuantity]))");
                    table.ForeignKey(
                        name: "FK_GoodsReceiptDetails_PODetails",
                        column: x => x.PurchaseOrderDetailId,
                        principalTable: "PurchaseOrderDetails",
                        principalColumn: "PurchaseOrderDetailId");
                    table.ForeignKey(
                        name: "FK_GoodsReceiptDetails_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                    table.ForeignKey(
                        name: "FK_GoodsReceiptDetails_Receipts",
                        column: x => x.GoodsReceiptId,
                        principalTable: "GoodsReceipts",
                        principalColumn: "GoodsReceiptId");
                    table.ForeignKey(
                        name: "FK_GoodsReceiptDetails_Units",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId");
                });

            migrationBuilder.CreateTable(
                name: "PurchasePriceHistories",
                columns: table => new
                {
                    PurchasePriceHistoryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderDetailId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchasePriceHistories", x => x.PurchasePriceHistoryId);
                    table.ForeignKey(
                        name: "FK_PurchasePriceHistories_Currencies",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "CurrencyId");
                    table.ForeignKey(
                        name: "FK_PurchasePriceHistories_PODetails",
                        column: x => x.PurchaseOrderDetailId,
                        principalTable: "PurchaseOrderDetails",
                        principalColumn: "PurchaseOrderDetailId");
                    table.ForeignKey(
                        name: "FK_PurchasePriceHistories_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                    table.ForeignKey(
                        name: "FK_PurchasePriceHistories_Suppliers",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId");
                });

            migrationBuilder.CreateTable(
                name: "CustomerReturnDetails",
                columns: table => new
                {
                    CustomerReturnDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerReturnId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<long>(type: "bigint", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ConditionNote = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerReturnDetails", x => x.CustomerReturnDetailId);
                    table.ForeignKey(
                        name: "FK_CustomerReturnDetails_Lots",
                        column: x => x.LotId,
                        principalTable: "InventoryLots",
                        principalColumn: "LotId");
                    table.ForeignKey(
                        name: "FK_CustomerReturnDetails_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                    table.ForeignKey(
                        name: "FK_CustomerReturnDetails_Returns",
                        column: x => x.CustomerReturnId,
                        principalTable: "CustomerReturns",
                        principalColumn: "CustomerReturnId");
                });

            migrationBuilder.CreateTable(
                name: "DeliveryDetails",
                columns: table => new
                {
                    DeliveryDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryId = table.Column<long>(type: "bigint", nullable: false),
                    SalesOrderDetailId = table.Column<long>(type: "bigint", nullable: false),
                    LotId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryDetails", x => x.DeliveryDetailId);
                    table.ForeignKey(
                        name: "FK_DeliveryDetails_Deliveries",
                        column: x => x.DeliveryId,
                        principalTable: "Deliveries",
                        principalColumn: "DeliveryId");
                    table.ForeignKey(
                        name: "FK_DeliveryDetails_Lots",
                        column: x => x.LotId,
                        principalTable: "InventoryLots",
                        principalColumn: "LotId");
                    table.ForeignKey(
                        name: "FK_DeliveryDetails_SalesOrderDetails",
                        column: x => x.SalesOrderDetailId,
                        principalTable: "SalesOrderDetails",
                        principalColumn: "SalesOrderDetailId");
                });

            migrationBuilder.CreateTable(
                name: "WarehouseShelves",
                columns: table => new
                {
                    ShelfId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RackId = table.Column<int>(type: "int", nullable: false),
                    ShelfCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseShelves", x => x.ShelfId);
                    table.ForeignKey(
                        name: "FK_WarehouseShelves_Racks",
                        column: x => x.RackId,
                        principalTable: "WarehouseRacks",
                        principalColumn: "RackId");
                });

            migrationBuilder.CreateTable(
                name: "QualityInspectionDetails",
                columns: table => new
                {
                    QualityInspectionDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QualityInspectionId = table.Column<long>(type: "bigint", nullable: false),
                    GoodsReceiptDetailId = table.Column<long>(type: "bigint", nullable: false),
                    CheckName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Result = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityInspectionDetails", x => x.QualityInspectionDetailId);
                    table.CheckConstraint("CK_QualityInspectionDetails_Result", "(([Result]='NotApplicable' OR [Result]='Pending' OR [Result]='Fail' OR [Result]='Pass'))");
                    table.ForeignKey(
                        name: "FK_QualityInspectionDetails_Inspections",
                        column: x => x.QualityInspectionId,
                        principalTable: "QualityInspections",
                        principalColumn: "QualityInspectionId");
                    table.ForeignKey(
                        name: "FK_QualityInspectionDetails_ReceiptDetails",
                        column: x => x.GoodsReceiptDetailId,
                        principalTable: "GoodsReceiptDetails",
                        principalColumn: "GoodsReceiptDetailId");
                });

            migrationBuilder.CreateTable(
                name: "WarehouseBins",
                columns: table => new
                {
                    BinId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShelfId = table.Column<int>(type: "int", nullable: false),
                    BinCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Capacity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "((1))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseBins", x => x.BinId);
                    table.ForeignKey(
                        name: "FK_WarehouseBins_Shelves",
                        column: x => x.ShelfId,
                        principalTable: "WarehouseShelves",
                        principalColumn: "ShelfId");
                });

            migrationBuilder.CreateTable(
                name: "InventoryBalances",
                columns: table => new
                {
                    BalanceId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotId = table.Column<long>(type: "bigint", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false, defaultValueSql: "((0))"),
                    QuantityReserved = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false, defaultValueSql: "((0))"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryBalances", x => x.BalanceId);
                    table.CheckConstraint("CK_InventoryBalances_Reserved", "(([QuantityReserved]<=[QuantityOnHand]))");
                    table.ForeignKey(
                        name: "FK_InventoryBalances_Bins",
                        column: x => x.BinId,
                        principalTable: "WarehouseBins",
                        principalColumn: "BinId");
                    table.ForeignKey(
                        name: "FK_InventoryBalances_Lots",
                        column: x => x.LotId,
                        principalTable: "InventoryLots",
                        principalColumn: "LotId");
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: true),
                    TransactionType = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    QuantityChange = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReferenceType = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    ReferenceId = table.Column<long>(type: "bigint", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CreatedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.TransactionId);
                    table.CheckConstraint("CK_InventoryTransactions_Type", "(([TransactionType]='Return' OR [TransactionType]='Adjustment' OR [TransactionType]='TransferIn' OR [TransactionType]='TransferOut' OR [TransactionType]='Issue' OR [TransactionType]='Receipt'))");
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Bins",
                        column: x => x.BinId,
                        principalTable: "WarehouseBins",
                        principalColumn: "BinId");
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Employees",
                        column: x => x.CreatedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Lots",
                        column: x => x.LotId,
                        principalTable: "InventoryLots",
                        principalColumn: "LotId");
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Warehouses",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustmentDetails",
                columns: table => new
                {
                    AdjustmentDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjustmentId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<long>(type: "bigint", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    SystemQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    DifferenceQuantity = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: true, computedColumnSql: "([ActualQuantity]-[SystemQuantity])", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustmentDetails", x => x.AdjustmentDetailId);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDetails_Adjustments",
                        column: x => x.AdjustmentId,
                        principalTable: "StockAdjustments",
                        principalColumn: "AdjustmentId");
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDetails_Bins",
                        column: x => x.BinId,
                        principalTable: "WarehouseBins",
                        principalColumn: "BinId");
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDetails_Lots",
                        column: x => x.LotId,
                        principalTable: "InventoryLots",
                        principalColumn: "LotId");
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDetails_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                });

            migrationBuilder.CreateTable(
                name: "StockCountDetails",
                columns: table => new
                {
                    StockCountDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockCountId = table.Column<long>(type: "bigint", nullable: false),
                    LotId = table.Column<long>(type: "bigint", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    SystemQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    DifferenceQuantity = table.Column<decimal>(type: "decimal(19,3)", precision: 19, scale: 3, nullable: true, computedColumnSql: "([ActualQuantity]-[SystemQuantity])", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockCountDetails", x => x.StockCountDetailId);
                    table.ForeignKey(
                        name: "FK_StockCountDetails_Bins",
                        column: x => x.BinId,
                        principalTable: "WarehouseBins",
                        principalColumn: "BinId");
                    table.ForeignKey(
                        name: "FK_StockCountDetails_Lots",
                        column: x => x.LotId,
                        principalTable: "InventoryLots",
                        principalColumn: "LotId");
                    table.ForeignKey(
                        name: "FK_StockCountDetails_StockCounts",
                        column: x => x.StockCountId,
                        principalTable: "StockCounts",
                        principalColumn: "StockCountId");
                });

            migrationBuilder.CreateTable(
                name: "StockReservations",
                columns: table => new
                {
                    ReservationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotId = table.Column<long>(type: "bigint", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderDetailId = table.Column<long>(type: "bigint", nullable: true),
                    ReservedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValueSql: "('Active')"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockReservations", x => x.ReservationId);
                    table.CheckConstraint("CK_StockReservations_Status", "(([Status]='Cancelled' OR [Status]='Fulfilled' OR [Status]='Released' OR [Status]='Active'))");
                    table.ForeignKey(
                        name: "FK_StockReservations_Bins",
                        column: x => x.BinId,
                        principalTable: "WarehouseBins",
                        principalColumn: "BinId");
                    table.ForeignKey(
                        name: "FK_StockReservations_Lots",
                        column: x => x.LotId,
                        principalTable: "InventoryLots",
                        principalColumn: "LotId");
                });

            migrationBuilder.CreateTable(
                name: "StockTransferDetails",
                columns: table => new
                {
                    TransferDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<long>(type: "bigint", nullable: false),
                    SourceBinId = table.Column<int>(type: "int", nullable: false),
                    DestinationBinId = table.Column<int>(type: "int", nullable: false),
                    RequestedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false, defaultValueSql: "((0))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransferDetails", x => x.TransferDetailId);
                    table.CheckConstraint("CK_StockTransferDetails_Received", "(([ReceivedQuantity]<=[RequestedQuantity]))");
                    table.ForeignKey(
                        name: "FK_StockTransferDetails_DestinationBins",
                        column: x => x.DestinationBinId,
                        principalTable: "WarehouseBins",
                        principalColumn: "BinId");
                    table.ForeignKey(
                        name: "FK_StockTransferDetails_Lots",
                        column: x => x.LotId,
                        principalTable: "InventoryLots",
                        principalColumn: "LotId");
                    table.ForeignKey(
                        name: "FK_StockTransferDetails_Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                    table.ForeignKey(
                        name: "FK_StockTransferDetails_SourceBins",
                        column: x => x.SourceBinId,
                        principalTable: "WarehouseBins",
                        principalColumn: "BinId");
                    table.ForeignKey(
                        name: "FK_StockTransferDetails_Transfers",
                        column: x => x.TransferId,
                        principalTable: "StockTransfers",
                        principalColumn: "TransferId");
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "CountryId", "CountryCode", "CountryName", "IsActive" },
                values: new object[] { 1, "VN", "Vietnam", true });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "CurrencyId", "CurrencyCode", "CurrencyName", "IsActive" },
                values: new object[] { 1, "VND", "Vietnamese Dong", true });

            migrationBuilder.InsertData(
                table: "CustomerTypes",
                columns: new[] { "CustomerTypeId", "TypeCode", "TypeName" },
                values: new object[] { 1, "RETAIL", "Retail" });

            migrationBuilder.InsertData(
                table: "Units",
                columns: new[] { "UnitId", "IsActive", "UnitCode", "UnitName" },
                values: new object[] { 1, true, "EA", "Each" });

            migrationBuilder.CreateIndex(
                name: "IX_Brands_BrandName_CountryId",
                table: "Brands",
                columns: new[] { "BrandName", "CountryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_CountryCode",
                table: "Countries",
                column: "CountryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_CountryName",
                table: "Countries",
                column: "CountryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_CurrencyCode",
                table: "Currencies",
                column: "CurrencyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReturns_ReturnNumber",
                table: "CustomerReturns",
                column: "ReturnNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerCode",
                table: "Customers",
                column: "CustomerCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTypes_TypeCode",
                table: "CustomerTypes",
                column: "TypeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_DeliveryNumber",
                table: "Deliveries",
                column: "DeliveryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_CurrencyId_RateDate",
                table: "ExchangeRates",
                columns: new[] { "CurrencyId", "RateDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_ReceiptNumber",
                table: "GoodsReceipts",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_Warehouse_Date",
                table: "GoodsReceipts",
                columns: new[] { "WarehouseId", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBalances_BinId",
                table: "InventoryBalances",
                column: "BinId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBalances_LotId_BinId",
                table: "InventoryBalances",
                columns: new[] { "LotId", "BinId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLots_Product_Expiry",
                table: "InventoryLots",
                columns: new[] { "ProductId", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLots_ProductId_LotCode",
                table: "InventoryLots",
                columns: new[] { "ProductId", "LotCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_Lot_Date",
                table: "InventoryTransactions",
                columns: new[] { "LotId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Manufacturers_CountryId",
                table: "Manufacturers",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Manufacturers_ManufacturerCode",
                table: "Manufacturers",
                column: "ManufacturerCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_PaymentId_SalesOrderId",
                table: "PaymentDetails",
                columns: new[] { "PaymentId", "SalesOrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentNumber",
                table: "Payments",
                column: "PaymentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CategoryCode",
                table: "ProductCategories",
                column: "CategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnits_ProductId_UnitId",
                table: "ProductUnits",
                columns: new[] { "ProductId", "UnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductWarehousePolicies_ProductId_WarehouseId",
                table: "ProductWarehousePolicies",
                columns: new[] { "ProductId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_PONumber",
                table: "PurchaseOrders",
                column: "PONumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_Supplier_Date",
                table: "PurchaseOrders",
                columns: new[] { "SupplierId", "OrderDate" });

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspections_InspectionNumber",
                table: "QualityInspections",
                column: "InspectionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_Customer_Date",
                table: "SalesOrders",
                columns: new[] { "CustomerId", "OrderDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_SalesOrderNumber",
                table: "SalesOrders",
                column: "SalesOrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustments_AdjustmentNumber",
                table: "StockAdjustments",
                column: "AdjustmentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockCountDetails_StockCountId_LotId_BinId",
                table: "StockCountDetails",
                columns: new[] { "StockCountId", "LotId", "BinId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockCounts_StockCountNumber",
                table: "StockCounts",
                column: "StockCountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_Status_Date",
                table: "StockTransfers",
                columns: new[] { "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_TransferNumber",
                table: "StockTransfers",
                column: "TransferNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierProducts_SupplierId_ProductId_ManufacturerId",
                table: "SupplierProducts",
                columns: new[] { "SupplierId", "ProductId", "ManufacturerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CountryId",
                table: "Suppliers",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_SupplierCode",
                table: "Suppliers",
                column: "SupplierCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_SettingKey",
                table: "SystemSettings",
                column: "SettingKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_UnitCode",
                table: "Units",
                column: "UnitCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseAisles_ZoneId_AisleCode",
                table: "WarehouseAisles",
                columns: new[] { "ZoneId", "AisleCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseBins_ShelfId_BinCode",
                table: "WarehouseBins",
                columns: new[] { "ShelfId", "BinCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseEmployees_WarehouseId_EmployeeId_AssignedFrom",
                table: "WarehouseEmployees",
                columns: new[] { "WarehouseId", "EmployeeId", "AssignedFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseRacks_AisleId_RackCode",
                table: "WarehouseRacks",
                columns: new[] { "AisleId", "RackCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_WarehouseCode",
                table: "Warehouses",
                column: "WarehouseCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseShelves_RackId_ShelfCode",
                table: "WarehouseShelves",
                columns: new[] { "RackId", "ShelfCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseZones_WarehouseId_ZoneCode",
                table: "WarehouseZones",
                columns: new[] { "WarehouseId", "ZoneCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CustomerReturnDetails");

            migrationBuilder.DropTable(
                name: "DeliveryDetails");

            migrationBuilder.DropTable(
                name: "ExchangeRates");

            migrationBuilder.DropTable(
                name: "FoodSafetyDocuments");

            migrationBuilder.DropTable(
                name: "InventoryBalances");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "ManufacturerDocuments");

            migrationBuilder.DropTable(
                name: "PaymentDetails");

            migrationBuilder.DropTable(
                name: "ProductStorageRequirements");

            migrationBuilder.DropTable(
                name: "ProductUnits");

            migrationBuilder.DropTable(
                name: "ProductWarehousePolicies");

            migrationBuilder.DropTable(
                name: "PurchasePriceHistories");

            migrationBuilder.DropTable(
                name: "QualityInspectionDetails");

            migrationBuilder.DropTable(
                name: "StockAdjustmentDetails");

            migrationBuilder.DropTable(
                name: "StockCountDetails");

            migrationBuilder.DropTable(
                name: "StockReservations");

            migrationBuilder.DropTable(
                name: "StockTransferDetails");

            migrationBuilder.DropTable(
                name: "SupplierContacts");

            migrationBuilder.DropTable(
                name: "SupplierDocuments");

            migrationBuilder.DropTable(
                name: "SupplierProducts");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "WarehouseEmployees");

            migrationBuilder.DropTable(
                name: "WarehouseTemperatureLogs");

            migrationBuilder.DropTable(
                name: "CustomerReturns");

            migrationBuilder.DropTable(
                name: "Deliveries");

            migrationBuilder.DropTable(
                name: "SalesOrderDetails");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "QualityInspections");

            migrationBuilder.DropTable(
                name: "GoodsReceiptDetails");

            migrationBuilder.DropTable(
                name: "StockAdjustments");

            migrationBuilder.DropTable(
                name: "StockCounts");

            migrationBuilder.DropTable(
                name: "WarehouseBins");

            migrationBuilder.DropTable(
                name: "InventoryLots");

            migrationBuilder.DropTable(
                name: "StockTransfers");

            migrationBuilder.DropTable(
                name: "CustomerAddresses");

            migrationBuilder.DropTable(
                name: "SalesOrders");

            migrationBuilder.DropTable(
                name: "PurchaseOrderDetails");

            migrationBuilder.DropTable(
                name: "GoodsReceipts");

            migrationBuilder.DropTable(
                name: "WarehouseShelves");

            migrationBuilder.DropTable(
                name: "Manufacturers");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "WarehouseRacks");

            migrationBuilder.DropTable(
                name: "CustomerTypes");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "WarehouseAisles");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "WarehouseZones");

            migrationBuilder.DropTable(
                name: "Warehouses");
        }
    }
}
