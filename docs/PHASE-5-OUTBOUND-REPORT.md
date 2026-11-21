# Phase 5 — Outbound

## Pre-implementation inspection

- The Sales Order, Sales Order Detail, Customer, Customer Address, Inventory Balance, Inventory Lot, Inventory Transaction, Delivery, Delivery Detail, Stock Reservation, Employee, Warehouse Bin, Product Unit, Currency, Product, and Warehouse entities/configurations were already present.
- There was no outbound controller, service, API client, or page. `/outbound` used the generic in-development placeholder.
- No migration or database/schema change was made.
- Read-only inspection confirmed existing data: 1 sales order with 2 lines, 1 delivery with 1 line, and 1 active stock reservation. Those records were not modified.

## Implemented

- Added outbound options for active customers and their addresses, currencies, products/units, warehouses, and employees.
- Added paginated/searchable sales-order and delivery lists, plus order and delivery details.
- Added draft sales-order creation with reference, quantity, unit, price, discount, and exchange-rate validation.
- Added order confirmation with explicit lot/bin allocations. The service validates product/lot, warehouse/bin, address ownership, expiration, active unit conversion, exact undelivered quantities, and available stock. It creates stock reservations and a draft delivery in a serializable transaction.
- Added delivery preparation, shipment confirmation, delivered status, and pre-shipment cancellation. Shipment reduces on-hand and reserved quantities together and writes negative `Issue` inventory transactions in the same transaction. Repeated shipment is rejected.
- Added an `/outbound` UI for order creation, lot/bin allocation, order and delivery details, and delivery status actions. Payments are not processed by this module.

## API

- `GET /api/outbound/options`
- `GET /api/outbound/availability?warehouseId=&productId=&unitId=`
- `GET /api/outbound/orders` and `GET /api/outbound/orders/{id}`
- `POST /api/outbound/orders`
- `POST /api/outbound/orders/{id}/confirm`
- `POST /api/outbound/orders/{id}/cancel`
- `GET /api/outbound/deliveries` and `GET /api/outbound/deliveries/{id}`
- `POST /api/outbound/deliveries/{id}/prepare`
- `POST /api/outbound/deliveries/{id}/ship`
- `POST /api/outbound/deliveries/{id}/delivered`
- `POST /api/outbound/deliveries/{id}/cancel`

## Files created or changed

- Backend: `CommercialWms.Api/Contracts/Outbound/OutboundContracts.cs`, `CommercialWms.Api/Controllers/OutboundController.cs`, `CommercialWms.Api/Services/IOutboundService.cs`, `CommercialWms.Api/Services/OutboundService.cs`; registered the service in `CommercialWms.Api/Program.cs`.
- Tests: `CommercialWms.Api.Tests/OutboundServiceTests.cs`.
- Frontend: `CommercialWms.Web/src/pages/OutboundPage.tsx`, `CommercialWms.Web/src/services/outboundApi.ts`; added contracts to `CommercialWms.Web/src/types/contracts.ts`, routed `/outbound` in `CommercialWms.Web/src/routes/AppRouter.tsx`, and moved the menu item to operational navigation in `CommercialWms.Web/src/layouts/AppLayout.tsx`.

## Verification

- Backend build: passed with 0 warnings and 0 errors. Output was directed to a temporary directory because the existing API process holds its normal build output open.
- Frontend lint: passed.
- Frontend production build: passed.
- Isolated service tests: 13/13 passed (7 outbound and 6 existing inventory tests). Test writes use EF Core InMemory, not `WareHouse`.
- Read-only API smoke test using the new build and the actual `WareHouse` connection: order list, delivery list, options, order detail, and availability returned successfully. The smoke-test instance ran on a temporary port.
- Frontend route `/outbound` returned HTTP 200. Its configured proxy to the already-running API at port 5206 returned HTTP 404 for the new route because that existing process is still running the older build.
- Final read-only counts were unchanged: SalesOrders 1, SalesOrderDetails 2, Deliveries 1, DeliveryDetails 1, StockReservations 1, InventoryBalances 21, InventoryTransactions 24. No API write request, migration, or database mutation was issued.

## Limitations and unverified behavior

- The live frontend-to-backend workflow is not yet available through the existing port 5206 process. Restart that process with the updated build before trying the UI against the new endpoints; it was left running and was not interrupted.
- No successful POST, reservation, shipment, or cancellation was sent to the real database. Those state-changing flows were tested only in the isolated service suite, as requested.
- EF Core InMemory ignores relational transaction semantics. SQL Server rollback behavior and concurrent shipment/reservation races still need a separately authorized integration test environment.
- `DeliveryDetails` has no bin column. The chosen bin is retained in `StockReservations` and used to write the shipment ledger; the schema cannot store a separate bin directly on each delivery line. `StockReservations.SalesOrderDetailId` also has no database FK, so the service validates the relationship itself.
