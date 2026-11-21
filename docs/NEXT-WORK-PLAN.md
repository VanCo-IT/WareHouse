# Commercial WMS — Next Work Plan

**Status:** Proposed sequence for Product Owner/team review; no sprint commitment or feature implementation is implied.  
**Basis:** [Implementation status](IMPLEMENTATION-STATUS.md) and [implementation matrix](IMPLEMENTATION-MATRIX.csv), audit snapshot 2026-11-19.

## Decisions required before scheduling

1. Confirm business actors, roles, access rules, and who may create/approve/cancel receipts, adjust/transfer stock, reserve/ship orders, and perform stock counts.
2. Confirm workflow state transitions, partial receipt/shipment rules, cancellation semantics, reservation expiry/release, FEFO/FIFO policy, unit conversion/rounding, and currency/exchange-rate behavior.
3. Decide whether the existing Shelf level satisfies the warehouse hierarchy or whether Floor requires a formally approved schema change. Do not add a Floor entity/table by assumption.
4. Confirm the required audit trail and traceability. The current schema does not directly link a StockReservation to a SalesOrderDetail and does not store a bin on DeliveryDetails.
5. Approve an isolated SQL Server integration-test database and test-data lifecycle. Keep shared `WareHouse` read-only during testing; never use database `A` or the legacy `CommercialWMS`.

## Prioritized sequence

### P0 — Establish a safe test and security baseline

- Create/authorize an isolated SQL Server test target using the existing schema; document its connection separately without changing the current application connection string.
- Add SQL Server integration tests for service transaction boundaries, rollback, FK/check behavior, and concurrent inventory reservation/adjustment/transfer. Keep the existing InMemory unit tests as fast logic tests, not as substitutes for SQL tests.
- Agree and implement authentication plus role/policy authorization before exposing operational write endpoints. Verify that every state-changing API has an explicit access policy.
- Add auditable test evidence and define which GET smoke checks may run against shared `WareHouse`.

**Exit criteria:** Tests run only on the approved isolated database; transaction and authorization failures are verified; shared database contents remain unchanged.

### P1 — Close foundational master-data gaps

- Implement customer/type/address management and employee/warehouse assignment after field, validation, and role rules are approved.
- Complete exchange-rate and product-unit management, including effective dates, conversion rules, and precision/rounding.
- Expose product warehouse thresholds (`MinQuantity`, `MaxQuantity`, `SafetyQuantity`) and product storage requirements through validated APIs and usable screens.
- Decide and implement manufacturer document handling. Keep file storage, type/size limits, and retention explicit.

**Dependencies:** P0 identity/permission decisions and PO rules for conversions, exchange rates, and documents.

### P2 — Verify and complete receiving

- Exercise purchase-order creation/approval, receipt creation, check-in, quality inspection, cancellation, and confirmation on isolated SQL Server fixtures.
- Test short/excess/damaged/rejected quantities, bin selection, lot/date validation, repeated confirmation, failure rollback, and status transitions.
- Prove that accepted receipts create/update lots and balances once and post corresponding InventoryTransactions atomically.
- Add API/service/UI integration coverage; no positive receiving workflow can be tested using the current shared snapshot because all five purchase orders are already `Received`.

**Dependencies:** P0 isolated database and authorization; P1 product-unit rules where receiving quantities require conversion.

### P3 — Verify inventory operations against SQL Server

- Run adjustment increase/decrease and transfer scenarios through the API on isolated fixtures.
- Verify available quantity, reservation interaction, no-negative balances, same-source/destination rejection, and exact ledger legs.
- Add failure injection/rollback and concurrent operation tests; reconcile transaction totals to resulting balances.
- Integrate and test the policy thresholds in low-stock calculations after P1 defines their semantics.

**Dependencies:** P0 integration environment and agreed inventory policy; successful P2 receipt posting for end-to-end stock provenance.

### P4 — Verify outbound order-to-delivery lifecycle

- Test create → confirm/reserve → prepare → ship → delivered on isolated SQL Server data, including retries, duplicate requests, cancellations, partial delivery, and insufficient stock.
- Verify that reservations are acquired/released exactly once and on-hand/reserved/available balances and InventoryTransactions reconcile at every state transition.
- Confirm order-line/lot/location traceability requirements and explicitly assess the current schema gaps before any schema proposal.
- Add browser-driven integration tests for the supported UI lifecycle without using shared database writes.

**Dependencies:** P0 identity/test environment; P1 customer/address management; P3 verified inventory rules.

### P5 — Implement remaining operational modules

After requirements are approved, implement and test in this order:

1. Stock-count lifecycle and approved variance posting.
2. Customer returns, inspection, restock decisions, and refund/payment association.
3. Payment capture/recording and refund/reconciliation.
4. Warehouse temperature logging and threshold alerts.
5. Audit-log capture/query with authenticated actor identity.

Do not treat existing rows or current statuses as evidence these capabilities are already implemented.

### P6 — Data-backed reports and system settings

- Define report formulas, date/time semantics, filters, and source tables; implement dashboard/stock valuation, receipt/shipment, warehouse stock, low-stock, and movement reports only after validating their underlying workflows.
- Use `ProductWarehousePolicies` for agreed min/max/safety thresholds; do not hardcode report values.
- Define which `SystemSettings` have real operational meaning, then implement the matching API/UI. Remove or relabel placeholder menu entries until they do.
- Test report totals against controlled fixtures and SQL queries; verify export output if Excel export is approved.

## Cross-cutting completion gates

- Each function must have an API contract, usable UI, validation/error behavior, and access policy where applicable.
- Build and lint success is necessary but not a business completion gate. A `DONE` status requires reproducible functional evidence and recorded environment/test scope.
- Record test run date, application version, database target, fixture setup, actual result, and any defect; never call seeded/demo rows “live workflow tests.”
- Reconcile the stale README/WMS traceability and API/data-map documents after the PO-approved implementation sequence is established. Preserve all original instructor materials in `Mau tai lieu`.

## Explicitly out of scope until approved

- Any migration, table/column/constraint change, database creation/deletion, or connection-string change.
- Any write to shared `WareHouse`, database `A`, or legacy `CommercialWMS`.
- Any data seeding or synthetic business transaction in the shared environment.
- Any Git commit or push.
