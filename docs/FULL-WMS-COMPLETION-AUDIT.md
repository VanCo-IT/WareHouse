# Commercial WMS — Full Completion Audit

**Audit date:** 2026-11-21  
**Scope:** `CommercialWms.Api`, `CommercialWms.Web`, `CommercialWms.Api.Tests`, EF Core migration/configuration, and read-only SQL Server metadata for `WareHouse`.  
**Purpose:** Inventory schema, API/UI coverage, verification evidence, remaining work, and implementation dependencies.  
**Safety:** No source/configuration changes were made. No migration, seed script, SQL write, commit, or push was run. Database `A` and legacy database `CommercialWMS` were not accessed.

## 1. Executive summary

| Measure | Audited result |
|---|---:|
| WMS design tables in `InitialCreate` and `WareHouse` | **56** |
| Entity classes | **56** |
| `DbSet` properties in `CommercialWmsDbContext` | **56** |
| Fluent configuration classes | **56** |
| Applied EF Core migrations | **1** (`InitialCreate`, EF Core 10.0.12) |
| SQL Server user-table objects | **58**: 56 WMS tables, `__EFMigrationsHistory`, and SQL Server diagram support table `sysdiagrams` |
| Foreign keys | **101**, enabled |
| CHECK constraints | **25**, enabled |
| DEFAULT constraints | **55** |
| Secondary unique indexes | **39** |
| Primary-key indexes | **56** |
| Other indexes | **10** |
| Total indexes on the 56 WMS tables | **105** |
| Computed columns | **2** |
| API endpoints declared by controllers | **112**: 45 GET, 32 POST, 18 PUT, 16 DELETE, 1 PATCH |
| React page source files | **15** (includes a shared reference-catalog page) |
| Concrete routed UI views | **16**: 14 feature views and 2 placeholders |
| Automated backend test cases | **13**, all using EF Core InMemory |

### Table status summary

Statuses below describe **feature readiness**, not whether the entity/table exists. Every design table is present in the migration and was found in the `WareHouse` schema.

| Status | Count | Interpretation |
|---|---:|---|
| COMPLETE | **0** | No full business workflow has been verified end-to-end through SQL Server writes. |
| PARTIAL | **24** | Useful API/UI or workflow implementation exists, but coverage or SQL integration verification is incomplete. |
| MISSING | **23** | The table-backed capability has no meaningful API/UI workflow yet. |
| INTERNAL | **9** | Child, association, or history table consumed through another feature; no standalone CRUD screen is expected. |
| **Total** | **56** | |

## 2. Evidence and validation

### Schema and migration

- `CommercialWmsDbContext` declares 56 `DbSet` properties; the project has 56 entity files and 56 configuration files.
- `20261116075251_InitialCreate.cs` contains 56 `CreateTable` operations, 56 primary keys, 101 foreign keys, 25 CHECK constraints, 49 secondary `CreateIndex` operations, 55 SQL defaults, and 2 computed columns.
- A read-only query explicitly connected to `WareHouse` returned 58 user-table objects: the 56 WMS tables plus `dbo.__EFMigrationsHistory` and `dbo.sysdiagrams`. The migration history contains `20261116075251_InitialCreate` with product version `10.0.12`.
- The two computed columns are `StockAdjustmentDetails.DifferenceQuantity` and `StockCountDetails.DifferenceQuantity`.
- `49` secondary indexes comprise 39 unique and 10 non-unique indexes. Adding 56 primary-key indexes gives 105 indexes across the WMS tables.
- The prior WMS business-rules guide records 40 unique indexes including primary keys. Current SQL Server metadata and the migration resolve that difference as 39 secondary unique indexes plus 56 primary-key indexes; the older total should not be reused.
- There is no `Floor` table/entity/API/UI. The implemented location hierarchy is Warehouse → Zone → Aisle → Rack → Shelf → Bin.

### Build, tests, and live read evidence

- Backend: `dotnet build CommercialWms.Api/CommercialWms.Api.csproj --no-restore` succeeded on 2026-11-21 with **0 warnings and 0 errors**.
- Backend tests: `dotnet test CommercialWms.Api.Tests/CommercialWms.Api.Tests.csproj --no-restore` passed **13/13** on 2026-11-21. The suite uses EF Core InMemory, not SQL Server.
- Frontend: `npm run lint` passed and `npm run build` passed on 2026-11-21.
- Previous read-only runtime evidence dated 2026-11-19: catalog, receiving, inventory, and outbound GET smoke checks returned HTTP 200; Dashboard, Receiving, Inventory and Outbound pages rendered database-backed rows from `WareHouse`. A separate 2026-11-19 run recorded status and latency for the requested 11 GET endpoints. These checks do not prove any write workflow.
- No SQL Server integration-test project or isolated SQL Server test database is currently used by the 13 tests. They do not prove SQL locking, transaction rollback, constraints, or concurrent-write behavior.
- No successful POST/PUT/PATCH/DELETE request was sent to `WareHouse` as part of this audit.

### Startup, security, and write paths

- `Program.cs` registers the DbContext and services, maps controllers, and does not call `Migrate`, `EnsureCreated`, `EnsureDeleted`, or a seed routine. No `Startup` class or hosted service was found.
- Four `HasData` seeds exist in EF configuration (Country, Currency, Unit, CustomerType); they are migration seed metadata and are not applied by application startup. SQL demo seed files are manual scripts and were not executed.
- The current application connection string remains in `appsettings.json` and targets `localhost\SQL2022` / `WareHouse`. It was not changed.
- `UseAuthorization()` is present, but no authentication scheme, `[Authorize]` attribute, or role/policy setup was found. Mutating endpoints are therefore not protected by an implemented identity/authorization flow.
- Current `InventoryService` adjustment and transfer operations update `InventoryBalances` and append `InventoryTransactions`; they do **not** create `StockAdjustments`/`StockAdjustmentDetails` or `StockTransfers`/`StockTransferDetails` documents.
- `PurchasePriceHistory` rows are written on purchase-order approval. No endpoint or screen reads that history.
- No audit-log writer/interceptor or audit-log API/UI was found.

## 3. Table-by-table implementation matrix

`API` lists implemented or related routes; “none” means no relevant endpoint was found. `UI` lists a screen or indicates none. An INTERNAL row may be read or changed through its parent feature; it is not a claim that the parent workflow is complete.

### 3.1 Product catalog and reference data

| STT | Table | Entity | Relationships (FK targets) | Backend API | Frontend UI | Status | Công việc còn thiếu |
|---:|---|---|---|---|---|---|---|
| 1 | Countries | Country | none | CRUD `/api/Countries` | `/countries` | PARTIAL | Read/list evidence exists; verify CRUD, duplicate handling, and referenced-delete behavior against isolated SQL. |
| 2 | Currencies | Currency | none | CRUD `/api/Currencies` | `/currencies` | PARTIAL | Currency CRUD exists; add exchange-rate workflow separately and verify write paths in isolation. |
| 3 | CustomerTypes | CustomerType | none | none | none | MISSING | Needed by Customer master; no customer/type management API or UI. |
| 4 | ProductCategories | ProductCategory | ProductCategories (self-parent) | CRUD `/api/Categories` | `/categories` | PARTIAL | Verify parent-chain rules, CRUD, and protected delete with isolated tests. |
| 5 | Brands | Brand | Countries | CRUD `/api/Brands` | `/brands` | PARTIAL | Verify CRUD and delete behavior when referenced by Products. |
| 6 | Products | Product | Brands, ProductCategories | CRUD `/api/Products` | `/products` | PARTIAL | Verify CRUD/uniqueness/reference guards in isolated SQL; product-unit and warehouse-policy management remain incomplete. |
| 7 | Manufacturers | Manufacturer | Countries | CRUD `/api/Manufacturers` | `/manufacturers` | PARTIAL | Verify writes/delete guards; manufacturer documents and lot/expiry lifecycle are not managed as a dedicated feature. |
| 8 | Units | Unit | none | CRUD `/api/Units` | `/units` | PARTIAL | Verify writes; no UI/API for managing per-product conversion mappings. |
| 9 | ProductUnits | ProductUnit | Products, Units | GET lookup through Receiving; included in `/api/outbound/options` | Lookups in Receiving and Outbound; no management page | PARTIAL | Add validated conversion management and tests for unit conversion/rounding; no dedicated create/update/delete API. |
| 10 | ProductStorageRequirements | ProductStorageRequirement | Products | none | none | MISSING | Expose and manage storage-temperature/valid-date requirements; no current feature API/UI. |
| 11 | ProductWarehousePolicies | ProductWarehousePolicy | Products only | none | none | MISSING | Implement min/max/safety policy API/UI and policy-based alert/report behavior. `WarehouseId` is not an FK despite being required and part of the unique index. |

### 3.2 Supplier and manufacturer management

| STT | Table | Entity | Relationships (FK targets) | Backend API | Frontend UI | Status | Công việc còn thiếu |
|---:|---|---|---|---|---|---|---|
| 12 | Suppliers | Supplier | Countries | CRUD `/api/Suppliers` | `/suppliers` | PARTIAL | Verify create/update/status/delete and referential rules through isolated SQL tests. |
| 13 | SupplierContacts | SupplierContact | Suppliers | CRUD `/api/suppliers/{supplierId}/contacts` | Supplier detail `/suppliers/:id` | PARTIAL | Verify CRUD and parent guards; no SQL-backed write tests. |
| 14 | SupplierProducts | SupplierProduct | Suppliers, Products, Manufacturers | CRUD `/api/Suppliers/{id}/products` and child routes | Supplier detail `/suppliers/:id` | PARTIAL | Verify association uniqueness, editing, and purchase-history protected deletes. |
| 15 | SupplierDocuments | SupplierDocument | Suppliers | none | none | MISSING | No document upload/metadata API or UI. |
| 16 | ManufacturerDocuments | ManufacturerDocument | Manufacturers | none | none | MISSING | No document lifecycle/API/UI. |
| 17 | FoodSafetyDocuments | FoodSafetyDocument | Manufacturers, Suppliers | none | none | MISSING | No food-safety document management, expiry alerts, or UI. |

### 3.3 Customer and employee management

| STT | Table | Entity | Relationships (FK targets) | Backend API | Frontend UI | Status | Công việc còn thiếu |
|---:|---|---|---|---|---|---|---|
| 18 | Customers | Customer | CustomerTypes | Lookup in `/api/outbound/options`; no CRUD | No customer page | MISSING | Implement customer master after agreeing validation, customer type, and address rules. |
| 19 | CustomerAddresses | CustomerAddress | Customers | Read as part of `/api/outbound/options`; no address CRUD | No customer/address page | MISSING | Add address maintenance/default selection and validation; currently only consumed by outbound. |
| 20 | Employees | Employee | none | Active-employee lookup in `/api/outbound/options`; no CRUD | No employee page | MISSING | Add employee master and identity/role/assignment rules. |
| 21 | WarehouseEmployees | WarehouseEmployee | Warehouses only | none | none | MISSING | Add assignment management. `EmployeeId` is not protected by a database FK. |

### 3.4 Warehouse management

| STT | Table | Entity | Relationships (FK targets) | Backend API | Frontend UI | Status | Công việc còn thiếu |
|---:|---|---|---|---|---|---|---|
| 22 | Warehouses | Warehouse | none | CRUD `/api/Warehouses`; GET `/{id}/structure` | `/warehouses` | PARTIAL | List/structure read path verified; test create/update/delete, references, and parent-child constraints on isolated SQL. |
| 23 | WarehouseZones | WarehouseZone | Warehouses | CRUD `/api/WarehouseZones` | `/warehouses` hierarchy | PARTIAL | Verify CRUD, hierarchy validation, and delete guards. |
| 24 | WarehouseAisles | WarehouseAisle | WarehouseZones | CRUD `/api/WarehouseAisles` | `/warehouses` hierarchy | PARTIAL | Verify CRUD and parent/dependency guards. |
| 25 | WarehouseRacks | WarehouseRack | WarehouseAisles | CRUD `/api/WarehouseRacks` | `/warehouses` hierarchy | PARTIAL | Verify CRUD and parent/dependency guards. |
| 26 | WarehouseShelves | WarehouseShelf | WarehouseRacks | CRUD `/api/WarehouseShelves` | `/warehouses` hierarchy | PARTIAL | Verify CRUD and parent/dependency guards. |
| 27 | WarehouseBins | WarehouseBin | WarehouseShelves | CRUD `/api/WarehouseBins` | `/warehouses` hierarchy; Receiving/Inventory lookups | PARTIAL | Verify occupied/referenced-bin restrictions and end-to-end location selection. No Floor level exists in schema. |
| 28 | WarehouseTemperatureLogs | WarehouseTemperatureLog | Warehouses only | none | none | MISSING | No log-entry/read API, temperature alert, or screen. `RecordedByEmployeeId` has no FK. |

### 3.5 Purchasing, receiving, and quality inspection

| STT | Table | Entity | Relationships (FK targets) | Backend API | Frontend UI | Status | Công việc còn thiếu |
|---:|---|---|---|---|---|---|---|
| 29 | PurchaseOrders | PurchaseOrder | Suppliers, Currencies, Employees | GET/POST list/detail and submit/approve/cancel under `/api/Receiving/purchase-orders` | `/receiving` | PARTIAL | List/read verified; create/approve/price capture/status and authorization need SQL integration tests. |
| 30 | PurchaseOrderDetails | PurchaseOrderDetail | PurchaseOrders, Products, Units | Nested in purchase-order API; no standalone line CRUD | `/receiving` order create/list | INTERNAL | Verify line validation and round-trip detail; no separate line-management screen is expected. |
| 31 | GoodsReceipts | GoodsReceipt | PurchaseOrders, Warehouses, Employees | GET/POST, check-in, quality, confirm, cancel under `/api/Receiving/receipts` | `/receiving` | PARTIAL | GET verified. Verify successful receipt lifecycle, partial/excess/rejected quantities, repeat confirmation, rollback, and stock posting in isolated SQL. |
| 32 | GoodsReceiptDetails | GoodsReceiptDetail | GoodsReceipts, PurchaseOrderDetails, Products, Units | Nested receipt detail/check-in/confirm | `/receiving` receipt detail and check-in | INTERNAL | Verify line quantity reconciliation through the parent receipt workflow. |
| 33 | QualityInspections | QualityInspection | GoodsReceipts, Employees | GET/detail and quality-inspection operation under Receiving | `/receiving` inspection dialog/detail | PARTIAL | Verify pass/fail/conditional behavior and whether each result blocks/allows inventory posting as intended. |
| 34 | QualityInspectionDetails | QualityInspectionDetail | QualityInspections, GoodsReceiptDetails | Nested quality-inspection API | `/receiving` inspection dialog/detail | INTERNAL | Verify required checks and persistence as part of isolated receipt workflow. |
| 35 | PurchasePriceHistories | PurchasePriceHistory | PurchaseOrderDetails, Products, Suppliers, Currencies | Written on PO approval; no read endpoint | No history view | INTERNAL | Add a history read/query in the appropriate purchasing/product view if required; verify approval produces one entry per line and no duplicates. |

### 3.6 Inventory, transfers, adjustments, counts, and lots

| STT | Table | Entity | Relationships (FK targets) | Backend API | Frontend UI | Status | Công việc còn thiếu |
|---:|---|---|---|---|---|---|---|
| 36 | InventoryLots | InventoryLot | Products, Manufacturers, Countries | Created on receipt confirmation; exposed through inventory/outbound reads; no lot CRUD | Receiving, Inventory, Outbound | INTERNAL | Add dedicated expiry/lot search or alert view only if required; verify lot provenance and date rules through SQL workflow tests. |
| 37 | InventoryBalances | InventoryBalance | InventoryLots, WarehouseBins | GET `/api/inventory`; adjusted/transferred through InventoryService | `/inventory` stock balances | PARTIAL | Read and totals verified; verify SQL transaction atomicity, reservations, and concurrent balance writes. |
| 38 | InventoryTransactions | InventoryTransaction | InventoryLots, Warehouses, WarehouseBins, Employees | GET `/api/inventory/transactions`; written by receiving/inventory/outbound | `/inventory` history | PARTIAL | History/read and InMemory paths exist; verify full SQL ledger-to-balance reconciliation and source-reference integrity. |
| 39 | StockAdjustments | StockAdjustment | Warehouses, Employees | No endpoint persists this table; `/api/inventory/adjustments` updates balance and ledger directly | `/inventory` adjustment dialog does not create adjustment document | MISSING | Decide whether adjustment documents are required; if so, persist document/header and lifecycle using existing schema, then test. |
| 40 | StockAdjustmentDetails | StockAdjustmentDetail | StockAdjustments, Products, InventoryLots, WarehouseBins | No endpoint persists this table | No adjustment-document detail UI | MISSING | Connect adjustment lines to existing document schema if required. `DifferenceQuantity` is computed and must not be written. |
| 41 | StockTransfers | StockTransfer | Warehouses (source and destination), Employees | No endpoint persists this table; `/api/inventory/transfers` changes balances and ledger only | `/inventory` transfer dialog does not create transfer document | MISSING | Decide/implement transfer document lifecycle with existing tables; verify statuses, both legs, and rollback. |
| 42 | StockTransferDetails | StockTransferDetail | StockTransfers, Products, InventoryLots, WarehouseBins (source and destination) | No endpoint persists this table | No transfer-document detail UI | MISSING | Connect transfer lines and receipt/status handling if required by approved business flow. |
| 43 | StockReservations | StockReservation | InventoryLots, WarehouseBins | Created/released/fulfilled through outbound operations; included in order detail | `/outbound` order detail | INTERNAL | Test SQL/concurrency/retry semantics. `SalesOrderDetailId` exists but has no FK. |
| 44 | StockCounts | StockCount | Warehouses, Employees | none | none | MISSING | Implement count lifecycle, freeze/recount/approval and variance posting after policy approval. |
| 45 | StockCountDetails | StockCountDetail | StockCounts, InventoryLots, WarehouseBins | none | none | MISSING | Implement line capture and variance review; `DifferenceQuantity` is computed, not an input. |

### 3.7 Sales, delivery, returns, and payments

| STT | Table | Entity | Relationships (FK targets) | Backend API | Frontend UI | Status | Công việc còn thiếu |
|---:|---|---|---|---|---|---|---|
| 46 | SalesOrders | SalesOrder | Customers, Currencies, Employees | GET/POST list/detail, confirm, cancel under `/api/outbound/orders` | `/outbound` | PARTIAL | Read and service rules exist; verify SQL create/reserve/ship/cancel lifecycle and authorization. |
| 47 | SalesOrderDetails | SalesOrderDetail | SalesOrders, Products, Units | Nested in order API | `/outbound` order detail/allocation | INTERNAL | Verify line pricing, discount, conversion, and delivered quantity through the parent workflow. |
| 48 | Deliveries | Delivery | SalesOrders, Warehouses, CustomerAddresses | GET/POST lifecycle (prepare/ship/delivered/cancel) under `/api/outbound/deliveries` | `/outbound` delivery history/detail | PARTIAL | GET verified; verify SQL state transitions, retry/idempotency and cancellation after reservation. |
| 49 | DeliveryDetails | DeliveryDetail | Deliveries, SalesOrderDetails, InventoryLots | Nested in delivery API | `/outbound` delivery detail | INTERNAL | No bin/location column exists on this table; confirm traceability requirements before proposing schema changes. |
| 50 | CustomerReturns | CustomerReturn | Customers, SalesOrders | none | none | MISSING | Define return inspection, restock, refund and order-link rules; then implement. |
| 51 | CustomerReturnDetails | CustomerReturnDetail | CustomerReturns, Products, InventoryLots | none | none | MISSING | Implement only with the approved return lifecycle. |
| 52 | Payments | Payment | Customers, Currencies | none | none | MISSING | No payment/refund API, UI, or reconciliation workflow. |
| 53 | PaymentDetails | PaymentDetail | Payments, SalesOrders | none | none | MISSING | Implement payment allocation/refund linkage with the approved rules. |

### 3.8 System and audit

| STT | Table | Entity | Relationships (FK targets) | Backend API | Frontend UI | Status | Công việc còn thiếu |
|---:|---|---|---|---|---|---|---|
| 54 | ExchangeRates | ExchangeRate | Currencies | none | No exchange-rate screen | MISSING | Define effective-date/source/base-currency semantics; add API/UI and validation. |
| 55 | SystemSettings | SystemSetting | none | none | `/settings` placeholder | MISSING | Define supported operational settings first; no nonfunctional controls or guessed defaults. |
| 56 | AuditLogs | AuditLog | Employees | No writer/interceptor, query API, or UI | No audit page | INTERNAL | Establish actor identity/audit policy, then implement capture and authorized querying. |

## 4. UI classification by requested business group

| # | Business group | Current UI | Coverage and missing items |
|---:|---|---|---|
| 1 | Dashboard | `/` — DashboardPage | Live catalog counters only; no operational stock/receiving/outbound KPIs or dashboard API. |
| 2 | Product Catalog | `/products`, `/categories`, `/brands` | Main CRUD screens exist. Product-unit conversion, storage requirements, and warehouse stock policies have no management UI. |
| 3 | Supplier Management | `/suppliers`, `/suppliers/:id`, `/manufacturers` | Supplier, contact/product associations and manufacturer screens exist. Supplier/manufacturer/food-safety documents are absent. |
| 4 | Customer Management | No page | Customers, customer types, and address maintenance are missing. Outbound options are lookups, not customer management. |
| 5 | Warehouse Management | `/warehouses` | Warehouse→Zone→Aisle→Rack→Shelf→Bin. No Floor entity exists; no temperature-log UI. |
| 6 | Receiving / Purchase Orders | `/receiving` | Order/receipt/inspection flow UI exists; successful write lifecycle against isolated SQL Server is unverified. |
| 7 | Inventory / Stock Management | `/inventory` | Stock search, lot/balance view and transaction history; min/max/safety policy management is not integrated. |
| 8 | Stock Transfer | Transfer dialog within `/inventory` | Changes balance and writes paired ledger entries; does not create `StockTransfers` documents/details. |
| 9 | Stock Adjustment | Adjustment dialog within `/inventory` | Changes balance and writes a ledger entry; does not create `StockAdjustments` documents/details. |
| 10 | Outbound / Sales Orders | `/outbound` | Order creation, allocation/reservation and outbound actions exist; SQL end-to-end writes remain unverified. |
| 11 | Delivery Management | Delivery history/detail within `/outbound` | Read and lifecycle UI exists; bin-level shipment location is not represented by DeliveryDetails schema. |
| 12 | Lot / Expiry Management | Lot fields shown in Receiving/Inventory/Outbound | No dedicated lot search/expiry dashboard or alert workflow. |
| 13 | Inventory Policies | No page | `ProductWarehousePolicies` and storage requirements have no API/UI. |
| 14 | Currency / Exchange Rates | `/currencies` | Currency CRUD exists; exchange rates are not managed. |
| 15 | Reports | `/reports` placeholder | No report API, data-backed report page, or export. |
| 16 | User / Role / Permission | No page or auth flow | No login, identity, role or authorization implementation found. |
| 17 | Audit / Activity Logs | No page | `AuditLogs` table has no application writer or query endpoint. Inventory transaction history is not a substitute for an actor audit log. |
| 18 | System Settings | `/settings` placeholder | `SystemSettings` has no API/UI and there is no defined operational settings contract. |

## 5. Business requirements assessment

| Requirement | Status | Evidence / gap |
|---|---|---|
| Receive goods from suppliers | PARTIAL | PO, receipt and quality APIs/UI exist; read path is verified, successful SQL write lifecycle is not. |
| Track brand and origin | PARTIAL | Brand-country and inventory-lot manufacturer/country relationships exist; catalog and receipt UI exist; full traceability workflow is not SQL-tested. |
| Preserve purchase-price history | PARTIAL | `PurchasePriceHistories` is inserted when a PO is approved; no read API/UI and no verified SQL approval test. |
| Multiple warehouses and storage locations | PARTIAL | Hierarchy API/UI and live read evidence exist. No Floor level; writes/delete protections need isolated verification. |
| Transfer between warehouses/locations | PARTIAL | Inventory API/UI adjusts two balances and writes paired transactions; stock-transfer header/detail tables are not used by that implementation. |
| Lot and expiry management | PARTIAL | Lots carry manufacturer/country/date data and are used by receiving/inventory/outbound; no dedicated expiry alert/management page. |
| On-hand, reserved, and available stock | PARTIAL | API/UI exposes these quantities; read evidence exists and reservation logic has InMemory tests, but no SQL integration test. |
| Minimum, maximum, and safety stock | MISSING | Schema has `ProductWarehousePolicies`; there is no policy API/UI or policy-driven low-stock report. |
| Sales orders and delivery | PARTIAL | APIs/UI and InMemory service tests exist; SQL lifecycle and idempotency are unverified. |
| Average cost | MISSING | No average-cost calculation/API/UI identified. Purchase price history alone is not average cost. |
| Total inventory valuation | MISSING | No cost/valuation endpoint, report, or UI identified. |
| Currency and exchange rates | PARTIAL | Currency CRUD exists; ExchangeRates has no API/UI and rate-selection behavior is undefined. |
| Receiving, shipping, and stock reports | MISSING | `/reports` is a placeholder; no reporting API or export found. |

## 6. API gaps

The 112 declared endpoints are concentrated in catalog, warehouse hierarchy, receiving, inventory operations, supplier subresources, and outbound. No endpoint group was found for:

1. Customer, CustomerType, and CustomerAddress maintenance.
2. Employee and warehouse-employee assignment maintenance; identity/authentication/roles.
3. ExchangeRates.
4. ProductUnits management (only lookups exist).
5. ProductStorageRequirements and ProductWarehousePolicies.
6. StockCount/StockCountDetails lifecycle.
7. CustomerReturns/CustomerReturnDetails.
8. Payments/PaymentDetails.
9. SupplierDocuments, ManufacturerDocuments, FoodSafetyDocuments.
10. WarehouseTemperatureLogs.
11. SystemSettings.
12. AuditLog capture/query.
13. Data-backed reports, valuation, low-stock alerts, and exports.
14. Dedicated lot/expiry management and purchase-price-history query.
15. Formal StockAdjustment/StockTransfer document lifecycles. Existing inventory endpoints update balances/ledger only and do not persist their similarly named document tables.

The endpoint count is source-derived, not a claim that all 112 routes have been runtime-tested. Live evidence covers selected GET endpoints only.

## 7. Structural/data-integrity observations

These are observations of the current schema, not proposals or schema changes:

- `ProductWarehousePolicies.WarehouseId` has no FK to `Warehouses`; uniqueness is `(ProductId, WarehouseId)`.
- `WarehouseEmployees.EmployeeId` has no FK to `Employees`.
- `WarehouseTemperatureLogs.RecordedByEmployeeId` has no FK to `Employees`.
- `StockReservations.SalesOrderDetailId` is nullable but has no FK to `SalesOrderDetails`.
- `InventoryTransactions.ReferenceType`/`ReferenceId` are generic references without a direct FK to their source documents. Adjustment/transfer service behavior currently uses these ledger references rather than formal stock document rows.
- `DeliveryDetails` has no bin/location column.
- `StockAdjustmentDetails.DifferenceQuantity` and `StockCountDetails.DifferenceQuantity` are computed columns.
- Existing table names differ from some older documents (`ProductCategories`, `WarehouseZones`, etc.); use the actual migration/entity names, not stale prose.

Any change to resolve a schema limitation requires an explicit product decision and separate approval; this audit did not modify schema.

## 8. Prioritized completion roadmap

### P0 — Security and isolated SQL verification

1. Approve an isolated SQL Server integration-test target and fixture lifecycle; do not write to shared `WareHouse`.
2. Implement authentication/authorization before exposing workflow mutations beyond a trusted local developer environment.
3. Add SQL Server integration tests for transaction rollback, FK/CHECK constraints, idempotency, and concurrent inventory reservation/adjustment/transfer.
4. Keep current InMemory tests as fast service tests only.

**Dependency:** PO approval for test database and actors/roles.

### P1 — Master-data foundations

1. Customer, customer type, and customer addresses.
2. Employee management, warehouse assignment, and user/role model.
3. Product-unit conversions.
4. Exchange-rate lifecycle.
5. Product storage and warehouse policies.
6. Supplier/manufacturer/food-safety documents.

**Dependency:** agreed field rules, identity/role policy, conversion and exchange-rate semantics.

### P2 — Complete receiving and provenance

1. Test PO create/submit/approve and receipt creation/check-in/inspection/confirmation on isolated SQL.
2. Test partial, over, damaged, rejected, invalid-date, repeated-confirmation, and rollback paths.
3. Verify lot, balance, transaction, and purchase-price-history consistency.
4. Decide if price history needs a query/UI.

**Dependency:** P0 and product-unit conversion rules.

### P3 — Reconcile inventory documents and operations

1. Decide if existing StockAdjustment and StockTransfer document tables are required operational records.
2. If approved, make current inventory adjustment/transfer endpoints persist those documents and lines; do not add tables.
3. Verify on-hand/reserved/available calculations, paired transfer legs, no negative available stock, SQL isolation and race behavior.
4. Define and integrate ProductWarehousePolicies alerts.

**Dependency:** P0, policy semantics, formal-document decision.

### P4 — Complete outbound and customer fulfillment

1. SQL-test order create → reservation → delivery preparation → ship → delivered/cancel.
2. Verify reservation release/fulfil exactly once and ledger/balance reconciliation.
3. Confirm requirements for lot/bin shipment traceability and reservation-to-order-line integrity before any schema proposal.
4. Integrate customer/address maintenance.

**Dependency:** P0, P1, and P3 inventory verification.

### P5 — Remaining operations

1. Stock-count lifecycle and approved variance posting.
2. Customer returns, inspection, restock, and refund.
3. Payments and reconciliation.
4. Warehouse temperature logging/alerts.
5. Audit-log capture/query tied to authenticated actors.

**Dependency:** business-rule and role approvals; P0 integration test environment.

### P6 — Reports and settings

1. Define source formulas and date/filter semantics for inventory valuation, receiving, outbound, warehouse balances, low-stock and movement reports.
2. Define average-cost method and currency conversion before implementing valuation.
3. Implement data-backed reports and verify totals against controlled fixtures.
4. Implement only settings with an approved operational meaning; remove/relabel placeholders until then.

**Dependency:** stable receiving/inventory/outbound workflows, cost/rate rules, and policy integration.

## 9. Files and evidence references

- Backend model: [CommercialWmsDbContext.cs](../CommercialWms.Api/Data/CommercialWmsDbContext.cs), [Entities](../CommercialWms.Api/Entities/), [Configurations](../CommercialWms.Api/Data/Configurations/).
- Migration: [InitialCreate](../CommercialWms.Api/Data/Migrations/20261116075251_InitialCreate.cs), [model snapshot](../CommercialWms.Api/Data/Migrations/CommercialWmsDbContextModelSnapshot.cs).
- Backend routes and services: [Controllers](../CommercialWms.Api/Controllers/), [Services](../CommercialWms.Api/Services/), [Contracts](../CommercialWms.Api/Contracts/).
- Frontend routes: [AppRouter.tsx](../CommercialWms.Web/src/routes/AppRouter.tsx), [pages](../CommercialWms.Web/src/pages/), [services](../CommercialWms.Web/src/services/).
- Automated tests: [InventoryServiceTests.cs](../CommercialWms.Api.Tests/InventoryServiceTests.cs), [OutboundServiceTests.cs](../CommercialWms.Api.Tests/OutboundServiceTests.cs).
- Prior project status and next-work evidence: [IMPLEMENTATION-STATUS.md](IMPLEMENTATION-STATUS.md), [NEXT-WORK-PLAN.md](NEXT-WORK-PLAN.md), and [IMPLEMENTATION-MATRIX.csv](IMPLEMENTATION-MATRIX.csv).
