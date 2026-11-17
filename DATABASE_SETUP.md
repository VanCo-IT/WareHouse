# Commercial WMS database setup

This guide covers the existing SQL Server Code First schema and the demo data for `CommercialWms.Api`.

## Database target and safety

- The application database is `WareHouse`.
- Configure the SQL Server instance for your machine; the example settings use a placeholder instance.
- Never point the application or seed script at database `A` or `CommercialWMS`.
- `databaseA.sql` is a reference file, not a schema deployment script.
- The API does not apply migrations or seed data automatically.
- Do not drop or recreate an existing shared database. Confirm the effective connection string and database name before running database commands.

## Configure SQL Server

From the repository root, copy the sanitized example settings if you do not already have a local settings file:

```powershell
Copy-Item CommercialWms.Api\appsettings.Example.json CommercialWms.Api\appsettings.json
```

Set `ConnectionStrings:CommercialWms` to your SQL Server instance and keep the database name `WareHouse`. For example:

```text
Server=localhost\SQLServerInstance;Database=WareHouse;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

You can keep local settings in the ignored `appsettings.json`, or use .NET User Secrets/environment variables. Never commit passwords, tokens, local connection strings, or secret files.

## Apply the existing schema migration

The schema is managed by EF Core migrations. The existing migration is `20261116075251_InitialCreate`; do not execute the reference SQL file or create a separate baseline.

From the repository root:

```powershell
dotnet restore CommercialWms.Api\CommercialWms.Api.csproj
dotnet build CommercialWms.Api\CommercialWms.Api.csproj
dotnet ef database update --project CommercialWms.Api\CommercialWms.Api.csproj --startup-project CommercialWms.Api\CommercialWms.Api.csproj
```

Before `database update`, verify that the resolved connection string points to the intended `WareHouse` instance. On a database where the migration is already recorded, EF Core reports no pending schema work.

## Apply core demo data

The core demo seed is an insert-only SQL Server script. It uses Unicode literals, checks stable codes before inserting, wraps writes in a transaction, and verifies its key inventory relationships before commit. It also checks `DB_NAME()` and aborts unless the current database is `WareHouse`.

Run it from the repository root with `sqlcmd`:

```powershell
sqlcmd -S "localhost\SQLServerInstance" -d "WareHouse" -E -C -f 65001 -b -i "CommercialWms.Api\Data\Seeds\CommercialWmsDemoData.sql"
```

Replace `localhost\SQLServerInstance` with the instance used in your local connection string. `-E` uses Windows Integrated Authentication; use your approved secret-management method if your environment requires SQL Authentication. The script is designed to be safely re-run and does not update or delete existing rows. It includes the existing `FROZEN-PORK-001` as a reference but does not change it.

The demo values (including exchange rates, supplier contacts, certificates, and employees) are fictional and intended only for development/demo use. Verify actual product-label storage requirements before operational use.

## Apply additional workflow demo data

Run this only after the core demo seed, and only when the 18 target tables for customers, orders, payments, deliveries, returns, adjustments, transfers, reservations, settings, temperature logs, and audit logs are empty. The script checks the database name, target-table state, and starting inventory before writing. It uses a transaction and rolls back on errors.

```powershell
sqlcmd -S "localhost\SQLServerInstance" -d "WareHouse" -E -C -f 65001 -b -i "CommercialWms.Api\Data\Seeds\CommercialWmsAdditionalDemoData.sql"
```

The script creates a linked order, delivery, payment, pending return request, approved stock adjustment, completed transfer, active reservation, ambient temperature readings, and audit records. Inventory balances are updated only alongside matching `InventoryTransactions`. The return remains pending inspection and is not added back to stock; the schema does not record a refund association. Temperature readings are labeled as ambient receiving-area readings because the schema has no zone-level temperature foreign key.

The seed is safe to rerun after a complete successful run: it verifies the marker, expected rows, and resulting balances, then exits without changing data. If it finds partial or unrelated rows in the target tables, or unexpected inventory, it stops instead of merging or overwriting them.

## Verify the database

Use read-only queries against `WareHouse`:

```sql
SELECT DB_NAME() AS CurrentDatabase;
SELECT MigrationId, ProductVersion FROM dbo.__EFMigrationsHistory;
SELECT CategoryCode, CategoryName FROM dbo.ProductCategories ORDER BY CategoryCode;
SELECT SKU, ProductName, CategoryId, BrandId FROM dbo.Products ORDER BY SKU;
SELECT WarehouseCode, WarehouseName FROM dbo.Warehouses ORDER BY WarehouseCode;
SELECT PONumber, Status FROM dbo.PurchaseOrders ORDER BY PONumber;
SELECT LotCode, ProductId, ManufactureDate, ExpiryDate FROM dbo.InventoryLots ORDER BY LotCode;
SELECT LotId, QuantityOnHand, QuantityReserved FROM dbo.InventoryBalances ORDER BY LotId;
SELECT StockCountNumber, Status FROM dbo.StockCounts ORDER BY StockCountNumber;
SELECT StockCountId, LotId, SystemQuantity, ActualQuantity, DifferenceQuantity
FROM dbo.StockCountDetails
ORDER BY StockCountId, LotId;
SELECT SalesOrderNumber, Status FROM dbo.SalesOrders ORDER BY SalesOrderNumber;
SELECT PaymentNumber, Amount, Status FROM dbo.Payments ORDER BY PaymentNumber;
SELECT ReturnNumber, Status FROM dbo.CustomerReturns ORDER BY ReturnNumber;
SELECT TransferNumber, Status FROM dbo.StockTransfers ORDER BY TransferNumber;
SELECT LotId, BinId, QuantityOnHand, QuantityReserved FROM dbo.InventoryBalances ORDER BY LotId, BinId;
```

The core workflow contains 20 products, 5 suppliers/manufacturers/warehouses, warehouse location hierarchies, purchase orders and receipts, quality checks, inventory lots/balances/receipt transactions, price history, documents, and stock counts. The additional seed adds a small connected sales and inventory-movement scenario without deleting existing data.

## Run and smoke-test the API

```powershell
dotnet run --project CommercialWms.Api\CommercialWms.Api.csproj --launch-profile http
```

The default HTTP profile listens at `http://localhost:5206`. Read-only list endpoints:

- `GET http://localhost:5206/api/categories`
- `GET http://localhost:5206/api/brands`
- `GET http://localhost:5206/api/products`
- `GET http://localhost:5206/api/suppliers`

No API write endpoint is required to apply the demo seed.
