# Commercial WMS API

CommercialWms.Api is an ASP.NET Core Web API for a commercial warehouse management system. It uses EF Core Code First to map the SQL Server schema and expose APIs for product catalog, suppliers, and related reference data.

## Technology

- .NET 10 (SDK 10.0.401 is pinned in `global.json`)
- ASP.NET Core Web API
- Entity Framework Core 10.0.12
- SQL Server
- EF Core migrations and per-entity Fluent API configurations

## Database configuration

The Code First database is **`WareHouse`**. The checked-in `CommercialWms.Api/appsettings.Example.json` uses a placeholder SQL Server instance and Windows Integrated Authentication; it contains no username or password. Local `appsettings.json` and `appsettings.Development.json` files are ignored by Git.

Create a local settings file from the example if one does not already exist, then replace `SQLServerInstance` with your own SQL Server instance. Keep the database name `WareHouse` unless the team explicitly agrees otherwise. Do not use database `A` or `CommercialWMS` as the application target.

```powershell
Copy-Item CommercialWms.Api\appsettings.Example.json CommercialWms.Api\appsettings.json
```

Recommended local override with User Secrets:

```powershell
dotnet user-secrets init --project CommercialWms.Api
dotnet user-secrets set "ConnectionStrings:CommercialWms" "Server=localhost\SQLServerInstance;Database=WareHouse;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True" --project CommercialWms.Api
```

Alternatively, set an environment variable for the current PowerShell session:

```powershell
$env:ConnectionStrings__CommercialWms = "Server=localhost\SQLServerInstance;Database=WareHouse;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
```

Keep SQL-authentication credentials in User Secrets or another approved secret store. Never commit passwords, secret files, or personal connection details.

`databaseA.sql` is the provided schema reference only. Do not execute it to create or update the Code First database. The application does not call `EnsureCreated`, `EnsureDeleted`, or apply migrations at startup.

The existing initial migration is `20261116075251_InitialCreate`. For a new local database, verify the resolved connection string and obtain team approval before running:

```powershell
dotnet ef database update --project CommercialWms.Api\CommercialWms.Api.csproj --startup-project CommercialWms.Api\CommercialWms.Api.csproj
```

This command creates or updates the database selected by the effective connection string. Do not run it against a shared or production database without approval.

## Restore, build, and run

From the workspace root:

```powershell
dotnet restore CommercialWms.Api\CommercialWms.Api.csproj
dotnet build CommercialWms.Api\CommercialWms.Api.csproj
dotnet run --project CommercialWms.Api\CommercialWms.Api.csproj --launch-profile http
```

The HTTP launch profile listens at `http://localhost:5206`. Use the URL printed by `dotnet run` if the port is changed.

## Implemented APIs

All API responses use request/response DTOs rather than returning EF entities directly.

### Products — `/api/products`

| Method | Route | Behavior |
| --- | --- | --- |
| GET | `/api/products` | List products |
| GET | `/api/products/{id}` | Product detail |
| POST | `/api/products` | Create; validates references and rejects duplicate SKU/barcode |
| PUT | `/api/products/{id}` | Update |
| DELETE | `/api/products/{id}` | Delete only when no dependent records reference the product |

Create and update requests require `sku`, `productName`, and an existing `categoryId`. `brandId`, `barcode`, and `description` are optional. A provided brand must exist; creating a product with an inactive brand is rejected. Create defaults `isPerishable` and `isActive` to `true` when omitted. Duplicate SKU/barcode returns `409 Conflict`; invalid input or references return `400 Bad Request`.

### Product categories — `/api/categories`

| Method | Route | Behavior |
| --- | --- | --- |
| GET | `/api/categories` | List categories |
| GET | `/api/categories/{id}` | Category detail |
| POST | `/api/categories` | Create category |
| PUT | `/api/categories/{id}` | Update category |
| DELETE | `/api/categories/{id}` | Delete only when no products or child categories reference it |

Category codes are required and unique. Category names are required and limited to 100 characters. A parent category must exist and cannot be the category itself.

### Brands — `/api/brands`

| Method | Route | Behavior |
| --- | --- | --- |
| GET | `/api/brands` | List brands |
| GET | `/api/brands/{id}` | Brand detail |
| POST | `/api/brands` | Create brand |
| PUT | `/api/brands/{id}` | Update brand |
| DELETE | `/api/brands/{id}` | Delete only when no products reference it |

Brands require an existing `countryId`. The database enforces uniqueness for `(BrandName, CountryId)`; SQL Server collation controls case sensitivity. Deactivating a brand does not alter existing products.

### Suppliers and contacts

| Method | Route | Behavior |
| --- | --- | --- |
| GET | `/api/suppliers` | List suppliers |
| GET | `/api/suppliers/{id}` | Supplier detail |
| POST | `/api/suppliers` | Create supplier |
| PUT | `/api/suppliers/{id}` | Update supplier |
| PATCH | `/api/suppliers/{id}/status` | Activate/deactivate supplier |
| GET | `/api/suppliers/{id}/products` | List linked products |
| POST | `/api/suppliers/{id}/products` | Link product and manufacturer |
| PUT | `/api/suppliers/{id}/products/{productId}` | Update link; provide `manufacturerId` when needed |
| DELETE | `/api/suppliers/{id}/products/{productId}` | Remove link only when purchase history permits |
| GET | `/api/suppliers/{supplierId}/contacts` | List supplier contacts |
| POST | `/api/suppliers/{supplierId}/contacts` | Create contact |
| PUT | `/api/suppliers/{supplierId}/contacts/{contactId}` | Update contact |
| DELETE | `/api/suppliers/{supplierId}/contacts/{contactId}` | Delete contact |

Supplier-product links are unique by supplier, product, and manufacturer. The existing schema has no supplier-product current-price column; the API does not invent one. Purchase-order and purchase-price-history values remain separate.

## Project structure

```text
CommercialWms.Api/
  Contracts/       Request and response DTOs grouped by API
  Controllers/     Products, categories, brands, suppliers, supplier contacts
  Data/
    Configurations/ Fluent API configuration for the entities
    Migrations/     EF Core migration, model snapshot, review script
    Seeds/          Data-only sample/recovery scripts
  Entities/         EF Core entities
  Properties/       Local launch profiles
  Program.cs        API and SQL Server dependency setup
  appsettings*.json Local application settings
README.md
databaseA.sql      Provided schema reference; not the application database target
global.json        .NET SDK selection
dotnet-tools.json  Local EF CLI tool manifest
```

There are 56 entity classes and 56 matching Fluent API configuration files. `CommercialWmsDbContext` discovers configurations from the project assembly. The model contains stable reference seeds for Countries, Currencies, Units, and CustomerTypes; transactional records are not seeded by the model.

## Handoff status

- **Implemented:** 56-table EF Core model and initial migration; Products, Categories, Brands, Suppliers, supplier-product links, and supplier contacts API endpoints.
- **Verified in this workspace:** `.NET 10.0.401`; `dotnet restore` and `dotnet build`; Products CRUD over HTTP using temporary test data that was removed afterward; Categories, Brands, and Products list GET endpoints against `WareHouse`.
- **Not covered by the current handoff verification:** full HTTP mutation testing for Categories, Brands, Suppliers, and supplier contacts; purchase-order, receiving, inventory, sales, delivery, returns, payments, stock-count, and system-management APIs.
- **Database:** the configured Code First target is `WareHouse`. API smoke tests use GET only; migrations and seed scripts are not run automatically.

Review the resolved connection string and `.gitignore` before preparing a commit. Do not commit `bin/`, `obj/`, `databaseA.sql`, local `appsettings*.json`, secret files, credentials, or personal connection settings.
