# Commercial WMS — Demo Readiness Audit

**Ngày rà soát:** 2026-11-21  
**Phạm vi:** `CommercialWms.Api`, `CommercialWms.Web`, `CommercialWms.Api.Tests` và [FULL-WMS-COMPLETION-AUDIT.md](./FULL-WMS-COMPLETION-AUDIT.md).  
**Giới hạn:** Chỉ đọc source và tài liệu; không khởi động dịch vụ, không gọi API, không truy cập SQL Server, không ghi dữ liệu, không sửa code/cấu hình, không migration/seed, không commit/push.

## 1. Tổng quan

**Hiện chưa thể demo trực tiếp ngay tại workspace:** kiểm tra trạng thái listener cho thấy không có tiến trình lắng nghe ở cổng `5206` (Backend) hoặc `5173` (Frontend). Theo yêu cầu, đợt rà soát này không khởi động chúng. Vì vậy không có kiểm tra API/browser mới; các bằng chứng runtime bên dưới được ghi rõ là kết quả lần kiểm tra trước, không phải xác nhận trạng thái hiện tại.

Các lần chạy kiểm tra tĩnh trong đợt này:

| Kiểm tra | Kết quả | Giới hạn |
|---|---|---|
| Backend `dotnet build ... --no-restore` | Thành công, 0 cảnh báo, 0 lỗi | Không chứng minh kết nối SQL hay nghiệp vụ ghi |
| Backend tests | 13/13 đạt | Tất cả dùng EF Core InMemory; không phải SQL Server integration tests |
| Frontend `npm run lint` | Thành công | Không kiểm tra API runtime |
| Frontend `npm run build` | Thành công | Không chứng minh trang tải dữ liệu thật |
| Trạng thái cổng 5206/5173 | Không có listener | Không gọi API và không mở browser trong lần này |

Mã khởi động trong [Program.cs](../CommercialWms.Api/Program.cs) đăng ký DbContext và các service, không tự gọi `Migrate`, `EnsureCreated`, `EnsureDeleted`, seed routine hoặc hosted service. Tuy nhiên, `UseAuthorization()` hiện không đi kèm authentication/role enforcement: không thấy `AddAuthentication` hay `[Authorize]`. Header giao diện có nhãn tĩnh “Quản trị viên / Tài khoản demo”, không phải đăng nhập thật.

### Cách hiểu trạng thái

- **DEMO READY:** Có bằng chứng runtime cho đúng phạm vi được nêu; không đồng nghĩa toàn bộ module hoàn chỉnh.
- **NEEDS FIX:** Có chức năng nhưng có vấn đề UX/hiển thị hoặc rủi ro cần khắc phục.
- **INCOMPLETE:** Có code/API/UI nhưng chưa xác minh luồng nghiệp vụ đầy đủ hoặc còn thiếu phần cốt lõi.
- **MISSING:** Chưa có chức năng.
- **NOT VERIFIED:** Chưa đủ bằng chứng runtime cho chức năng đó.

## 2. Đánh giá 10 nhóm nghiệp vụ

| # | Nhóm | Trạng thái | UI / API | Bằng chứng và khoảng trống |
|---:|---|---|---|---|
| 1 | Danh mục sản phẩm, thương hiệu, nhà cung cấp | **NOT VERIFIED** | UI: `/products`, `/categories`, `/brands`, `/suppliers`; API: `/api/Products`, `/api/Categories`, `/api/Brands`, `/api/Suppliers` | Source có form/API CRUD và validation. Smoke test GET trước đây ghi nhận các API catalog trả 200; chưa có bằng chứng hiện tại và chưa gửi thao tác ghi thành công. File: [ProductsPage.tsx](../CommercialWms.Web/src/pages/ProductsPage.tsx), [SuppliersPage.tsx](../CommercialWms.Web/src/pages/SuppliersPage.tsx), `CommercialWms.Api/Controllers/*Controller.cs`. |
| 2 | Kho và vị trí lưu trữ | **NOT VERIFIED** | UI: `/warehouses`; API: `/api/Warehouses`, `/api/WarehouseZones`, `/api/WarehouseAisles`, `/api/WarehouseRacks`, `/api/WarehouseShelves`, `/api/WarehouseBins` | Cây Warehouse → Zone → Aisle → Rack → Shelf → Bin có code; không có Floor trong schema. Báo cáo Khung 2 ghi lần kiểm tra UI khi API không chạy, nên không chứng minh dữ liệu kho hiển thị. File: [WarehousesPage.tsx](../CommercialWms.Web/src/pages/WarehousesPage.tsx), [PHASE-2-WAREHOUSE-REPORT.md](./PHASE-2-WAREHOUSE-REPORT.md). |
| 3 | Đơn mua hàng và nhập kho | **INCOMPLETE** | UI: `/receiving`; API: `/api/Receiving/purchase-orders`, `/api/Receiving/receipts` | Bằng chứng 2026-11-19: GET hoạt động và trang hiển thị 5 đơn mua, 5 phiếu nhập thật. Theo báo cáo lúc đó, toàn bộ đơn đã `Received`, phiếu `Accepted`; không có dữ liệu mở phù hợp để trình diễn luồng nhập thành công. Chưa có SQL write test. File: [ReceivingPage.tsx](../CommercialWms.Web/src/pages/ReceivingPage.tsx), [ReceivingService.cs](../CommercialWms.Api/Services/ReceivingService.cs). |
| 4 | Kiểm tra chất lượng | **INCOMPLETE** | Trong UI `/receiving`; API: `PUT /api/Receiving/receipts/{id}/quality-inspection`, `POST /api/Receiving/receipts/{id}/confirm` | Service yêu cầu phân loại đủ số nhận, có check cho từng dòng; chỉ kết quả `Passed` được nhập lượng chấp nhận. `Failed` chỉ kết thúc khi không có lượng được chấp nhận; `Conditional`/`Pending` bị chặn chờ quyết định nghiệp vụ. Không có automated test hoặc SQL-backed test cho các nhánh này; dữ liệu trước đây chỉ có phiếu đã chấp nhận. |
| 5 | Tồn kho, lô và lịch sử giao dịch | **DEMO READY — chỉ xem, theo bằng chứng runtime trước đây** | UI: `/inventory`; API: `GET /api/inventory`, `GET /api/inventory/transactions` | Ngày 2026-11-19, GET trả 200 và trang hiển thị số dư/lô/giao dịch từ `WareHouse`: 21 số dư, 20 lô, 24 giao dịch. Không được hiểu đây là xác nhận hiện tại; hiện không có service listener và lần rà soát này không đọc DB. File: [InventoryPage.tsx](../CommercialWms.Web/src/pages/InventoryPage.tsx), [InventoryService.cs](../CommercialWms.Api/Services/InventoryService.cs). |
| 6 | Điều chỉnh tồn kho | **INCOMPLETE** | UI: dialog trong `/inventory`; API: `POST /api/inventory/adjustments` | Có validation, kiểm tra khả dụng, cập nhật `InventoryBalances` và ghi `InventoryTransactions`; 3 test InMemory bao phủ tăng, giảm hợp lệ, giảm vượt khả dụng. Không có SQL write test. Service không ghi `StockAdjustments`/`StockAdjustmentDetails`, nên thiếu chứng từ tương ứng nếu cần trình diễn kiểm soát nghiệp vụ. |
| 7 | Chuyển kho | **NEEDS FIX** | UI: dialog trong `/inventory`; API: `POST /api/inventory/transfers` | 3 test InMemory xác nhận hai bút toán cân bằng và các trường hợp lỗi. Service chỉ cập nhật số dư và ledger, không ghi `StockTransfers`/`StockTransferDetails`. UI lấy vị trí đích theo kho nhưng không loại vị trí nguồn; chọn cùng bin sẽ bị API từ chối thay vì bị chặn ngay tại UI. Chưa xác minh SQL transaction. |
| 8 | Đơn bán hàng và giữ hàng | **INCOMPLETE** | UI: `/outbound`; API: `/api/outbound/orders`, `/api/outbound/orders/{id}/confirm`, `GET /api/outbound/availability` | Có code tạo đơn, phân bổ lot/bin, kiểm tra available stock và giữ chỗ. Test InMemory bao phủ tạo/duyệt/giữ hàng, thiếu tồn và phân bổ không đủ. Runtime trước đây chỉ xác nhận GET; không có SQL write verification. Không có trang CRUD khách hàng/địa chỉ; dropdown dùng dữ liệu lookup. |
| 9 | Giao hàng | **INCOMPLETE** | Trong UI `/outbound`; API: `/api/outbound/deliveries` và các route `prepare`, `ship`, `delivered`, `cancel` | Có code chuyển trạng thái, giải phóng giữ chỗ khi hủy trước xuất và trừ tồn/ghi ledger khi ship. Test InMemory bao phủ một số nhánh. Dữ liệu trước đây có 1 đơn bán, 1 phiếu giao, 1 reservation; không có lần ship thật trên SQL. `DeliveryDetails` không lưu bin trực tiếp. |
| 10 | Dashboard và báo cáo | **INCOMPLETE** | UI `/`; báo cáo `/reports` | Dashboard chỉ đếm Products, Categories, Brands, Suppliers qua GET; không có KPI kho/nhập/xuất. `/reports` là placeholder, chưa có report API. Dashboard trước đây đã render các số liệu danh mục; lần này không mở browser. File: [DashboardPage.tsx](../CommercialWms.Web/src/pages/DashboardPage.tsx), [AppRouter.tsx](../CommercialWms.Web/src/routes/AppRouter.tsx). |

Các dòng **DEMO READY** chỉ áp dụng cho chế độ xem tồn/lịch sử theo lần kiểm tra trước; không xác nhận giao dịch ghi hoặc trạng thái dịch vụ hiện tại.

## 3. Có thể trình diễn và chưa thể trình diễn

### Có thể trình diễn sau khi dịch vụ được khởi động và GET hoạt động

Các luồng chỉ đọc đã có bằng chứng trước đây:

1. Dashboard đếm danh mục thật (không phải KPI vận hành).
2. Danh sách đơn mua/phiếu nhập và xem lịch sử đã hoàn tất.
3. Tồn kho theo sản phẩm/vị trí/lô và lịch sử giao dịch.
4. Danh sách đơn bán/phiếu giao và chi tiết hiện có.

Đây là kịch bản xem dữ liệu, không phải xác nhận hiện tại có thể truy cập ngay. Không gọi các nút tạo/sửa/xóa/duyệt/điều chỉnh/chuyển/giữ hàng/xuất hàng trên database dùng chung.

### Có giao diện nhưng nghiệp vụ chưa được xác minh hoàn chỉnh

- Tạo/duyệt đơn mua, nhận hàng, kiểm nhận, kiểm tra chất lượng và put-away.
- Điều chỉnh/chuyển tồn kho.
- Tạo đơn bán, phân bổ/giữ hàng, chuẩn bị và xuất giao.
- CRUD danh mục và cấu trúc kho trên SQL Server.

Các thao tác này có API/UI nhưng chưa có SQL integration tests hoặc successful write evidence. Unit tests hiện chạy trên EF Core InMemory; provider này không kiểm tra transaction/locking/rollback của SQL Server.

### Còn thiếu

- Customer/CustomerType/CustomerAddress master UI/API; Employees và phân công nhân viên.
- Báo cáo nhập-xuất-tồn, định giá tồn và average cost.
- Quản lý ProductWarehousePolicies (min/max/safety), cảnh báo tồn thấp.
- Quy trình chứng từ độc lập cho adjustment/transfer; stock count.
- Returns, payments, exchange rates, audit log, system settings.
- Authentication/authorization thực sự; hiện nhãn người dùng trên UI chỉ là text tĩnh.

## 4. Dữ liệu demo đã biết và giới hạn

Đây là số liệu **đã được ghi nhận trong lần kiểm tra chỉ đọc trước đây ngày 2026-11-19**, không phải số liệu được truy vấn trong đợt rà soát hiện tại:

| Dữ liệu | Số lượng đã ghi nhận | Khả năng demo |
|---|---:|---|
| Purchase Orders | 5 | Lịch sử; theo báo cáo lúc đó đều `Received` |
| Goods Receipts | 5 | Lịch sử; theo báo cáo lúc đó đều `Accepted` |
| Inventory Balances | 21 | Xem tồn |
| Inventory Lots | 20 | Xem lot trong màn tồn |
| Inventory Transactions | 24 | Xem lịch sử |
| Sales Orders / Details | 1 / 2 | Xem đơn có sẵn |
| Deliveries / Details | 1 / 1 | Xem phiếu giao có sẵn |
| Active Stock Reservations | 1 | Xem giữ chỗ hiện có |

Không xác nhận những số liệu này còn nguyên tại thời điểm demo. Không có số lượng catalog/warehouse được ghi nhận trong bằng chứng đã xem. Không tạo dữ liệu giả hoặc dữ liệu demo mới trong audit này. Không có đơn mua mở hoặc receipt chưa hoàn tất được xác nhận để trình diễn luồng nhập thành công; cũng không có bằng chứng xác nhận các đơn bán/phiếu giao hiện có ở trạng thái phù hợp cho từng bước demo.

Migration/configuration có `HasData` cho Country `VN`, Currency `VND`, Unit `EA` và CustomerType `RETAIL`; đây là seed metadata trong mã, không phải truy vấn xác nhận các dòng hiện còn trong database.

## 5. Các vấn đề cần xử lý trước demo

| Ưu tiên | Phát hiện, file/API và bằng chứng | Hậu quả nếu không xử lý |
|---|---|---|
| **P0 — chặn demo trực tiếp hiện tại** | Không có listener ở `5206` và `5173` khi kiểm tra. Frontend proxy cấu hình trong [vite.config.ts](../CommercialWms.Web/vite.config.ts) trỏ tới `http://localhost:5206`; Backend startup ở [Program.cs](../CommercialWms.Api/Program.cs). | Trang không lấy được dữ liệu API; không thể trình bày dữ liệu thật ngay tại trạng thái hiện tại. Cần người vận hành khởi động sau khi xác nhận cấu hình đúng môi trường; audit này không khởi động thay. |
| **P0 — không được ghi vào DB dùng chung để diễn tập** | Các nút UI gọi POST/PUT/PATCH/DELETE: [receivingApi.ts](../CommercialWms.Web/src/services/receivingApi.ts), [inventoryApi.ts](../CommercialWms.Web/src/services/inventoryApi.ts), [outboundApi.ts](../CommercialWms.Web/src/services/outboundApi.ts), [catalogApi.ts](../CommercialWms.Web/src/services/catalogApi.ts). Không có SQL integration DB được phép dùng trong phạm vi audit. | Một lần nhấn có thể thay đổi dữ liệu WareHouse thật. Chỉ demo read-only cho đến khi PO phê duyệt rõ môi trường/dữ liệu có thể ghi; không dùng test InMemory để thay thế bằng chứng SQL. |
| **P1 — quy trình nhập hàng chưa có bằng chứng end-to-end** | [ReceivingService.cs](../CommercialWms.Api/Services/ReceivingService.cs), [ReceivingPage.tsx](../CommercialWms.Web/src/pages/ReceivingPage.tsx); POST/PUT dưới `/api/Receiving`. Dữ liệu gần nhất đều ở trạng thái hoàn tất; không có test nhận hàng/quality/put-away. | Không thể chứng minh trước giảng viên hàng chỉ vào tồn sau khi kiểm nhận và chất lượng; sai trạng thái/put-away có thể không được phát hiện trước demo. |
| **P1 — điều chỉnh/chuyển không tạo chứng từ nghiệp vụ** | [InventoryService.cs](../CommercialWms.Api/Services/InventoryService.cs); `POST /api/inventory/adjustments`, `POST /api/inventory/transfers`. Source cập nhật balance/transaction ledger nhưng không ghi hai nhóm bảng `StockAdjustments*`/`StockTransfers*`. | Nếu giảng viên đối chiếu chứng từ, màn hình giao dịch không có bản ghi header/detail tương ứng; khó audit lý do, trạng thái và người phê duyệt. |
| **P1 — thao tác chuyển cho phép chọn nguồn làm đích trong UI** | [InventoryPage.tsx](../CommercialWms.Web/src/pages/InventoryPage.tsx), dialog chuyển; `POST /api/inventory/transfers` từ chối cùng bin. | Demo có thể gặp lỗi validation sau khi bấm xác nhận; người dùng có thể tưởng chuyển kho không hoạt động. |
| **P1 — chưa có xác thực/phân quyền** | [Program.cs](../CommercialWms.Api/Program.cs), [AppLayout.tsx](../CommercialWms.Web/src/layouts/AppLayout.tsx). Chỉ thấy `UseAuthorization`, không có authentication/`[Authorize]`; tên “Quản trị viên” là nội dung tĩnh. | Người truy cập được API có thể gọi endpoint ghi mà không có kiểm soát vai trò; không được đưa API mở ra mạng chung trong demo. |
| **P1 — nhãn Dashboard có thể gây hiểu nhầm** | [DashboardPage.tsx](../CommercialWms.Web/src/pages/DashboardPage.tsx) luôn hiển thị “API trực tiếp”, kể cả khi query lỗi; hiện API không chạy. | Người xem có thể hiểu dữ liệu đang cập nhật dù request thất bại; lỗi chỉ hiện thêm banner. |
| **P2 — Dashboard/Báo cáo chưa phản ánh nghiệp vụ WMS** | [DashboardPage.tsx](../CommercialWms.Web/src/pages/DashboardPage.tsx), [AppRouter.tsx](../CommercialWms.Web/src/routes/AppRouter.tsx), `GET /api/inventory` (chỉ tồn), không có report API. `/reports` hiện placeholder. | Demo thiếu KPI nhập/xuất/tồn, giá trị hàng, hàng sắp hết hạn và xuất file; không nên giới thiệu như đã hoàn tất. |
| **P2 — thiếu master data khách hàng và chính sách tồn** | Không có customer/address CRUD; `GET /api/outbound/options` chỉ cung cấp lookup. `ProductWarehousePolicies` chưa có API/UI. | Khó chuẩn bị dữ liệu khách hàng cho đơn mới; không thể trình bày cảnh báo min/max/safety hoặc quy tắc tồn theo kho. |

## 6. Kịch bản demo đề xuất (read-only an toàn)

Chỉ thực hiện sau khi người vận hành xác nhận Backend đang trỏ đúng `WareHouse`, chạy hai ứng dụng trên loopback và GET thành công. Không thực hiện các nút ghi dữ liệu.

1. **Mở Dashboard:** giải thích đây là số lượng danh mục, chưa phải dashboard vận hành.
2. **Danh mục:** xem sản phẩm/brand/category/supplier; không tạo, sửa hoặc xóa.
3. **Kho và vị trí:** chỉ mở nếu GET `/api/Warehouses` và cấu trúc kho trả dữ liệu; không thay đổi hierarchy.
4. **Nhập hàng:** mở danh sách PO và receipts, nhấn mạnh dữ liệu đã nhận; không giả lập một PO mở hoặc bấm duyệt/xác nhận.
5. **Tồn kho:** trình bày on-hand/reserved/available, lot/location và lịch sử giao dịch; không điều chỉnh/chuyển.
6. **Xuất hàng:** xem đơn bán, giữ chỗ và delivery có sẵn; không tạo đơn/confirm/ship/cancel.
7. **Báo cáo:** nêu rõ Reports và Settings đang là placeholder; không trình bày như chức năng đã có.

Nếu cần demo đầy đủ giao dịch ghi, cần PO phê duyệt một môi trường và dữ liệu có thể ghi trước; chưa được thực hiện trong nhiệm vụ này.

## 7. Công việc theo mức ưu tiên

### P0 — Chặn demo

1. Xác nhận với người vận hành cấu hình và khởi động Backend/Frontend trên loopback; kiểm tra GET trước khi mở giao diện. Không thay connection string trong bước này.
2. Không dùng các thao tác ghi trên `WareHouse` để diễn tập. Cần PO quyết định một môi trường ghi được phê duyệt nếu muốn demo luồng mutation.
3. Chuẩn bị trình tự demo read-only và phương án dừng nếu bất kỳ GET nào lỗi; không tạo dữ liệu mẫu giả.

### P1 — Nên hoàn thiện trước demo

1. Tạo test/integration evidence cho receiving + quality + put-away, adjustment/transfer và outbound shipment trên môi trường được PO phê duyệt.
2. Quyết định có bắt buộc chứng từ StockAdjustment/StockTransfer không; nếu có, hoàn thiện persistence theo schema hiện có và kiểm thử consistency.
3. Ẩn/loại trừ bin nguồn khỏi danh sách đích trong UI chuyển hàng; bổ sung hướng dẫn khi lookup lỗi/rỗng.
4. Bổ sung authentication/authorization hoặc giới hạn demo ở môi trường loopback đóng; bỏ nhãn tài khoản giả gây hiểu nhầm.
5. Bổ sung test riêng cho QC Passed/Failed/Conditional và retry/idempotency của receipt/delivery.

### P2 — Có thể làm sau demo

1. Customer/address, employee và warehouse assignment management.
2. Reports, giá trị tồn/average cost, lot-expiry views và policy min/max/safety.
3. Stock count, returns, payments, exchange rates, audit-log query và settings theo yêu cầu PO.

## 8. Kết luận

- **Có thể demo ngay theo nghĩa runtime hiện tại:** Không — cả hai ứng dụng không chạy trong lần kiểm tra.
- **Có thể demo sau khi khởi động, ở chế độ chỉ đọc:** Có bằng chứng trước đây cho Dashboard danh mục, Receiving, Inventory và Outbound; thời điểm gần nhất được ghi nhận là 2026-11-19. Các trang danh mục và warehouse chưa được xác minh trình duyệt trong bằng chứng đã xem.
- **Có thể demo giao dịch ghi end-to-end trên SQL Server:** Chưa. Build và 13 test InMemory không thay thế kiểm thử SQL Server; không có successful write evidence và không có môi trường ghi demo được phê duyệt.
- **Rủi ro chính:** dịch vụ đang dừng; dữ liệu nghiệp vụ hiện có thiên về chứng từ đã hoàn tất; thiếu authorization; adjustment/transfer không tạo chứng từ tương ứng; báo cáo và một số master data còn thiếu.

Không có code, cấu hình, database hoặc migration nào bị thay đổi trong audit này. Báo cáo là deliverable duy nhất được tạo.
