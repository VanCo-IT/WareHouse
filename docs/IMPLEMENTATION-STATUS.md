# Commercial WMS — Implementation Status

**Audit snapshot:** 2026-11-19  
**Scope:** Read-only audit of the current workspace, API, React application, tests, and SQL Server database `WareHouse`. No feature code was implemented as part of this audit.

## Executive summary

The matrix breaks the system into 37 auditable functions. Presence of an Entity, table, endpoint, page, or demo row is not by itself evidence that a workflow is complete.

| Status | Count | Meaning in this audit |
|---|---:|---|
| DONE | 2 | The specific read-only function or schema baseline has direct verification evidence. |
| IMPLEMENTED | 15 | Backend and UI code exist (or the automated service behavior is tested), but end-to-end business writes have not been verified against an isolated SQL Server database. |
| PARTIAL | 7 | Some layers or steps exist, but required workflow coverage, API/UI capability, or verification is incomplete. |
| UI ONLY | 2 | A route/menu is present but renders a development placeholder without a corresponding feature API. |
| API ONLY | 0 | No independently useful feature was identified as having an API but no corresponding feature UI; lookup endpoints used inside other workflows are noted in the matrix. |
| NOT STARTED | 9 | Schema may exist, but no feature API/UI implementation was found. |
| BLOCKED | 2 | Progress requires an approved schema/product decision or a separate SQL integration-test environment. |
| **Total** | **37** | Counts are function-level matrix rows, not table counts. |

**Completed:** 2 functions. **Not fully complete:** 35 functions (including implemented-but-not-fully-verified, partial, placeholders, not-started, and blocked items).

## Evidence and validation

### Workspace and builds

- The working tree was already dirty before this audit, with prior implementation changes, untracked test/source files, assignment materials, and documentation. These unrelated changes were preserved. No commit or push was made.
- The backend is a .NET 10 Web API with EF Core SQL Server; the frontend is React/TypeScript/Vite. No solution file was found; the API and test project are separate projects.
- The backend contains 56 Entity classes, 56 Fluent API configurations, and a `CommercialWmsDbContext` mapping the existing schema.
- `dotnet build CommercialWms.Api/CommercialWms.Api.csproj --no-restore` succeeded: **0 warnings, 0 errors**.
- `npm run lint` succeeded.
- `npm run build` succeeded.
- `dotnet test CommercialWms.Api.Tests/CommercialWms.Api.Tests.csproj --no-restore` succeeded: **13/13 tests** (6 inventory service tests and 7 outbound service tests). These use EF Core InMemory, not SQL Server.
- No frontend test script or browser automation test suite was found.

### Database and API/UI checks

- All database checks used read-only queries against **`WareHouse` only**. Database `A` and the old `CommercialWMS` database were not accessed. No migration or API write request was run.
- SQL Server reports **56 business tables**, **101 trusted foreign keys**, **25 trusted CHECK constraints**, **55 defaults**, **95 unique indexes**, and **2 computed columns**. `InitialCreate` is recorded in `__EFMigrationsHistory`.
- The two computed columns are `StockAdjustmentDetails.DifferenceQuantity` and `StockCountDetails.DifferenceQuantity`. The 95 unique-index count is from the current SQL metadata; an older WMS document states a different count (40), so that document should not be used as current evidence.
- The current snapshot contains **464 rows** across the business tables, including demo data. Every table is populated, but those rows do not prove that a user completed the corresponding workflow.
- Current workflow snapshot: 5 purchase orders are `Received`, 5 goods receipts are `Accepted`, 5 quality inspections exist, 1 sales order is `PartiallyDelivered`, 1 delivery is `Delivered`, 1 stock reservation is `Active`, and 5 stock counts are `Completed`. These are database states only, not newly executed workflow tests.
- Read-only API smoke checks returned HTTP 200 for catalog lists (categories, brands, products, suppliers, countries, currencies, units, manufacturers), warehouse lists/structure, purchase orders and receipts, inventory stock/transactions, and outbound options/availability/orders/deliveries. Representative detail GETs also returned 200.
- Browser smoke checks showed real database-backed data on `/warehouses` (5 warehouses), `/receiving` (5 receipts and 5 purchase orders), `/inventory` (21 balance rows), and `/outbound` (existing order). This verifies read/display paths, not form submissions or the operational write lifecycle.
- Temporary API/frontend instances used for the smoke checks were stopped and their dedicated temporary build/test output folders were removed. Ports 5206, 5210, and 5174 were confirmed stopped afterward.

### What the current tests do not prove

- The 13 automated tests exercise service logic with EF Core InMemory; they do not prove SQL Server transaction rollback, locking/isolation, FK behavior, concurrency safety, or a full HTTP-to-database workflow.
- No successful POST/PUT/PATCH/DELETE request was sent to `WareHouse`. No production/shared database write was performed.
- Receiving, inventory adjustment/transfer, and outbound mutation flows therefore remain unverified end to end against SQL Server. Existing demo rows are not substitutes for isolated fixtures.

## Findings by domain

### Catalog and reference data

Backend CRUD and React screens exist for categories, brands, products, suppliers, countries, currencies, units, and manufacturers. Supplier contacts and supplier-product/manufacturer associations are available through supplier detail. API list GETs were live-tested; builds passed. Full CRUD happy paths and delete/reference constraints were not re-tested in an isolated database during this audit.

Known incomplete reference-data coverage:

- `ExchangeRates` has schema/data but no dedicated API/UI.
- `ProductUnits` has schema and is used by receiving/product-unit lookup, but no independent management UI/API was found.
- `ProductWarehousePolicies` supports `MinQuantity`, nullable `MaxQuantity`, and `SafetyQuantity`. This resolves an inconsistency in the older Phase 4 report, which claimed the schema lacked thresholds; the policy table currently has no API/UI.
- `ProductStorageRequirements` and manufacturer documents have schema but no management workflow.

### Warehouses and locations

Warehouse/zone/aisle/rack/shelf/bin CRUD APIs and a hierarchy page exist. `GET /api/warehouses/{id}/structure` and list endpoints returned 200; the browser displayed five warehouses. There is **no Floor table/entity**; the supported hierarchy is Warehouse → Zone → Aisle → Rack → Shelf → Bin. Do not add a floor level without an approved schema/business decision.

### Receiving and inventory

Receiving has APIs for purchase orders, receipts, check-in, quality inspection, confirmation, and cancellation, plus a React receiving page. GETs and invalid-input behavior were checked in earlier phase evidence; the current live GETs and page rendering also passed. All current purchase orders are received, so no eligible open order was available for a positive live receiving workflow. No receiving mutation or inventory posting was sent to the shared database.

Inventory balance and transaction APIs/UI exist. The stock list was verified by API and browser against current data. Adjustment and transfer service tests pass in EF Core InMemory, but neither a SQL Server transaction nor a full integrated UI write has been verified. This is the boundary between **verified stock display** and **unverified operational inventory writes**.

### Sales, delivery, and outbound

Outbound APIs/UI cover options, availability, order list/detail/create, confirmation/reservation, cancellation, delivery preparation, shipment, delivery completion, and delivery cancellation. GET endpoints and the order list page returned/rendered successfully. Seven InMemory service tests cover core reservation/shipment behavior, rejection cases, cancellation/repeat behavior, and warehouse mismatch. No successful SQL Server write or browser-driven mutation lifecycle was run.

Known reconciliation gaps from the existing schema/reports: `StockReservations` has no FK to `SalesOrderDetails`, and delivery details do not store a bin/location. These constrain direct reservation/order-line and shipment/location reconciliation and should be resolved as a product/schema decision before claiming audit-grade traceability. No schema change is proposed by this audit.

### Remaining domains

- Customers/customer types/addresses and employee/warehouse assignments have schema but no management feature. Existing customer/employee lookup data is exposed as part of outbound options only.
- Returns, payments, stock-count operations, audit-log viewing, and warehouse temperature logging have no feature API/UI.
- `/reports` and `/settings` are registered routes but render the generic **“Đang phát triển”** placeholder. No reporting or settings API was found.
- There is no configured authentication scheme, login flow, `[Authorize]` use, or role/policy authorization. `UseAuthorization()` alone does not establish authentication or protect the mutation endpoints. The avatar text “Quản trị viên / Tài khoản demo” is static UI, not an authenticated identity.

## Documentation and traceability issues

- The old `docs/IMPLEMENTATION-STATUS.md` and `docs/IMPLEMENTATION-MATRIX.csv` predated the warehouse, receiving, inventory, and outbound implementations; they are replaced by this audit and the updated matrix.
- `README.md`, `docs/WMS Business Documentation/01-overview-and-scope.md`, `02-actors-and-use-cases.md`, `05-functional-decomposition.md`, `06-implementation-traceability.md`, and `07-api-and-data-map.md` contain stale statements that workflows lack APIs/tests or that the test project does not exist. Reconcile them after Product Owner review; they were not edited in this audit.
- PMA1012 materials are planning/submission templates, not proof of completed sprints. The Y2 backlog labels later workflow stories as proposals; the Y3 workbook cases remain `NOT RUN`. Preserve the original files in `Mau tai lieu`; do not mark assignment evidence complete from this technical audit.
- The API README and database setup guide contain older handoff statements and should be reconciled with the current API/test inventory. Local configuration/connection strings were not changed.

## Decision

The catalog, warehouse read paths, inventory read path, receiving lists, and outbound read paths are present and build. Only stock browsing and the schema baseline meet the `DONE` evidence bar in the matrix. The next steps are to approve actors/business rules, establish a separate SQL Server integration-test database, secure the write APIs, and verify receiving → inventory → outbound workflows there. The itemized priority order is in [NEXT-WORK-PLAN.md](NEXT-WORK-PLAN.md).
