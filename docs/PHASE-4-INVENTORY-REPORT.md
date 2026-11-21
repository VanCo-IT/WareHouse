# Phase 4 - Inventory

## Status

Giai đoạn 2 quản lý tồn kho đã được triển khai tiếp theo trên code hiện có của `CommercialWms.Api` và `CommercialWms.Web`, sử dụng database `WareHouse`. Không tạo migration, không thay đổi Entity/Configuration, không thay đổi connection string và không INSERT/UPDATE/DELETE dữ liệu thật.

## Capabilities already implemented

### Stock view

- API đọc tồn theo sản phẩm, kho, vị trí và từ khóa tìm kiếm.
- Phân trang bằng `page` và `pageSize`.
- Lọc chỉ còn khả dụng.
- Hiển thị tồn thực tế, giữ chỗ, khả dụng, lô và vị trí.
- Hiển thị nhà sản xuất, quốc gia, ngày sản xuất/hạn dùng khi dữ liệu schema có.
- Giao diện `/inventory` có tab số dư tồn và bộ lọc kho/vị trí/sản phẩm/từ khóa.

### Transaction history

- API đọc lịch sử `InventoryTransactions`.
- Hiển thị loại nhập, xuất, điều chỉnh, chuyển đi, chuyển đến và trả hàng.
- Lọc theo kho, sản phẩm, loại giao dịch, từ ngày và đến ngày.
- Phân trang lịch sử.
- Giao diện tab lịch sử giao dịch.

### Adjustment

- API điều chỉnh tăng hoặc giảm tồn.
- Kiểm tra lot/bin tồn tại và bin thuộc kho đang hoạt động.
- Kiểm tra employee nếu được cung cấp.
- Kiểm tra lý do bắt buộc, quantity khác 0, tối đa 3 chữ số thập phân.
- Không cho giảm tồn dưới quantity reserved.
- Ghi `TransactionType = Adjustment` trong cùng transaction database.

### Transfer

- API chuyển lot giữa hai bin hoặc hai kho.
- Kiểm tra source/destination khác nhau.
- Kiểm tra bin nguồn và đích hoạt động.
- Kiểm tra tồn khả dụng ở bin nguồn.
- Kiểm tra destination không vượt tối đa precision.
- Cập nhật cả bin nguồn và bin đích trong transaction.
- Ghi một bút toán `TransferOut` và một bút toán `TransferIn`.

## Backend files

- `CommercialWms.Api/Contracts/Inventory/InventoryContracts.cs`
- `CommercialWms.Api/Controllers/InventoryController.cs`
- `CommercialWms.Api/Services/IInventoryService.cs`
- `CommercialWms.Api/Services/InventoryService.cs`
- `CommercialWms.Api/Program.cs` đăng ký `IInventoryService`.

## Frontend files

- `CommercialWms.Web/src/pages/InventoryPage.tsx`
- `CommercialWms.Web/src/services/inventoryApi.ts`
- `CommercialWms.Web/src/types/contracts.ts`
- `CommercialWms.Web/src/routes/AppRouter.tsx`
- `CommercialWms.Web/src/layouts/AppLayout.tsx`
- `CommercialWms.Web/src/index.css`

## Database tables

- `InventoryBalances`
- `InventoryLots`
- `InventoryTransactions`
- `Products`
- `Warehouses`
- `WarehouseZones`
- `WarehouseAisles`
- `WarehouseRacks`
- `WarehouseShelves`
- `WarehouseBins`
- `Manufacturers`
- `Countries`
- `Employees`

Các thao tác điều chỉnh/chuyển kho không thay đổi schema. Giao dịch chuyển kho không tạo bảng hoặc Entity mới.

## Build verification

- Backend: after briefly stopping the API process, `dotnet build "CommercialWms.Api/CommercialWms.Api.csproj" --no-restore` succeeded with 0 warnings and 0 errors. The service was then restarted.
- Frontend lint: `npm run lint` succeeded.
- Frontend build: `npm run build` succeeded.

## Automated business-logic tests

Added `CommercialWms.Api.Tests/CommercialWms.Api.Tests.csproj` and `CommercialWms.Api.Tests/InventoryServiceTests.cs`. Tests use a unique EF Core InMemory database per case and never connect to SQL Server.

`dotnet test CommercialWms.Api.Tests/CommercialWms.Api.Tests.csproj --no-restore`: **6 passed, 0 failed, 0 skipped**.

Covered cases:

1. Adjustment increase updates the balance and writes a reasoned positive adjustment transaction.
2. Adjustment decrease within available quantity updates the balance and writes a negative adjustment transaction.
3. Adjustment decrease beyond available quantity is rejected with conflict and leaves balance/ledger unchanged.
4. Transfer across bins in separate warehouses updates source and destination, preserves total on-hand, and writes linked, balanced `TransferOut`/`TransferIn` entries.
5. Transfer to the source bin is rejected and leaves stock unchanged.
6. Transfer exceeding source available quantity is rejected with conflict and leaves both balances/ledger unchanged.

The EF Core InMemory provider ignores relational transactions; its warning is suppressed in the test context. These tests establish service rules and resulting balance/ledger state but do not validate SQL Server locking, isolation, or rollback under concurrent requests. Positive mutations were not sent to the live API.

## Runtime verification

- Backend: `http://localhost:5206`
- Frontend: `http://127.0.0.1:5173`
- API `/api/inventory?page=1&pageSize=25`: HTTP 200.
- API `/api/inventory/transactions?page=1&pageSize=25`: HTTP 200.
- Filter ngày đảo ngược: HTTP 400.
- `page=0`: HTTP 400.
- POST adjustment payload trống: HTTP 400.
- POST adjustment payload lot/bin/quantity/lý do không hợp lệ: HTTP 400.
- POST transfer với source/destination cùng bin: HTTP 400.
- Frontend `/inventory`: HTTP 200 and rendered real stock rows from `WareHouse`.
- Frontend stock tab displayed 21 database balance rows in the live smoke output and real lot/location data.
- Frontend history tab and date-filter controls were included in the final build; the browser stock view was visually verified.

## Database consistency verification

Read-only SQL against `WareHouse` showed:

- 21 inventory balance rows.
- 20 inventory lots.
- 24 inventory transactions.
- 0 rows with `QuantityReserved > QuantityOnHand`.

No write request that would alter inventory was sent. The source/destination and adjustment POST smoke tests were invalid payload cases only.

The post-test read-only counts match the previously observed state: 21 balances, 20 lots and 24 inventory transactions; reserved quantity exceeds on-hand in 0 rows.

## Implemented versus not completed

### Has API and UI

- Stock list by product, warehouse and bin.
- Lot information.
- Available/on-hand/reserved quantities.
- Search, filtering and pagination.
- Transaction history.
- Adjustment UI/API.
- Transfer UI/API.

### Automated in-memory tests pass; live write-flow is not verified

- Adjustment increase/decrease and transfers passed service-level tests against the isolated in-memory provider.
- No positive adjustment or transfer was performed against `WareHouse`; real SQL Server transaction isolation and concurrent behavior require an approved disposable test database.

The inventory business rules are tested without touching shared data, but the service is not reported as SQL Server integration-verified.

### Not implemented

- Minimum, maximum or safety-stock columns/configuration are absent from the schema.
- Low-stock/near-expiry warning calculations cannot be marked complete because the database has no threshold fields and no separate policy configuration.
- Authentication/authorization and reservation lifecycle were not part of this test suite.
- Role-based authorization is not implemented in the application.
- Reservation creation/reservation release is not implemented as a separate outbound workflow because that workflow belongs to a later business module and no existing API contract was found.

## Remaining confirmation

- PO must decide whether negative adjustments are allowed and whether a stock count approval workflow is required before final adjustment.
- PO must decide whether low-stock and expiry warnings use a fixed threshold or product-level configuration.
- A test database or explicitly approved writable transaction in `WareHouse` is required for positive adjustment and transfer verification.
