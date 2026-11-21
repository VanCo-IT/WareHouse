# Phase 3 — Nhập hàng

## Trạng thái

Đã triển khai luồng nhập hàng đầu tiên trên schema hiện có của `WareHouse`. Không tạo migration, thay đổi cấu trúc database, hoặc thêm dữ liệu nghiệp vụ.

## Chức năng

- Tra cứu đơn mua và phiếu nhập hiện có.
- Tạo đơn mua nháp với nhà cung cấp, tiền tệ, sản phẩm, đơn vị tính, số lượng, đơn giá và tỷ giá.
- Gửi duyệt, duyệt và hủy đơn mua theo trạng thái hợp lệ.
- Tạo phiếu nhập từ đơn mua đã duyệt hoặc nhận một phần.
- Ghi nhận số lượng thực nhận, chấp nhận và từ chối; kiểm tra precision và giới hạn số lượng.
- Ghi nhận các kiểm tra chất lượng theo từng dòng hàng.
- Chỉ ghi tồn kho khi người dùng xác nhận put-away sau khi kiểm tra chất lượng đạt.
- Put-away hàng vào lô và vị trí trong đúng kho; ghi nhận nhà sản xuất, xuất xứ, ngày sản xuất/hạn dùng và mã lô nhà cung cấp khi có.
- Ngăn ghi tồn kho trùng cho cùng phiếu nhập và ngăn tổng lượng chấp nhận vượt lượng đặt mua.
- Hủy phiếu nhập khi còn ở trạng thái nháp hoặc đang kiểm nhận.

Tồn khả dụng chỉ được cập nhật trong bước xác nhận cuối cùng. Bước này dùng transaction `Serializable` để cùng lúc ghi lô hàng, số dư tồn kho, giao dịch tồn kho và trạng thái phiếu.

## API

Base path: `/api/Receiving`

| Method | Route | Mục đích |
|---|---|---|
| GET | `/purchase-orders?status=` | Danh sách đơn mua, lọc trạng thái |
| GET | `/purchase-orders/{id}` | Chi tiết đơn mua |
| POST | `/purchase-orders` | Tạo đơn mua nháp |
| POST | `/purchase-orders/{id}/submit` | Gửi duyệt |
| POST | `/purchase-orders/{id}/approve` | Duyệt đơn mua |
| POST | `/purchase-orders/{id}/cancel` | Hủy đơn mua |
| GET | `/products/{productId}/units` | Lấy đơn vị được cấu hình cho sản phẩm |
| GET | `/warehouses/{warehouseId}/bins` | Lấy vị trí hoạt động trong kho |
| GET | `/receipts` | Danh sách phiếu nhập, hỗ trợ tìm kiếm và lọc |
| GET | `/receipts/{id}` | Chi tiết phiếu nhập và kiểm tra chất lượng |
| POST | `/receipts` | Tạo phiếu nháp từ đơn mua được duyệt |
| PUT | `/receipts/{id}/check-in` | Ghi nhận số lượng kiểm nhận |
| PUT | `/receipts/{id}/quality-inspection` | Ghi nhận kết quả kiểm tra chất lượng |
| POST | `/receipts/{id}/confirm` | Xác nhận chất lượng và put-away vào tồn kho |
| POST | `/receipts/{id}/cancel` | Hủy phiếu nhập chưa hoàn tất |

DTO riêng được dùng cho request/response; API không trả trực tiếp Entity. API xử lý lỗi đầu vào, tham chiếu không hợp lệ, trạng thái không hợp lệ và bản ghi không tồn tại bằng HTTP status phù hợp.

## Bảng được sử dụng

`PurchaseOrders`, `PurchaseOrderDetails`, `PurchasePriceHistories`, `Suppliers`, `SupplierProducts`, `Currencies`, `Employees`, `Products`, `Units`, `ProductUnits`, `GoodsReceipts`, `GoodsReceiptDetails`, `QualityInspections`, `QualityInspectionDetails`, `InventoryLots`, `InventoryBalances`, `InventoryTransactions`, `Manufacturers`, `Countries`, `Warehouses`, `WarehouseZones`, `WarehouseAisles`, `WarehouseRacks`, `WarehouseShelves`, `WarehouseBins`.

Do `GoodsReceiptDetails` không có khóa trực tiếp tới lô/vị trí, giao dịch tồn kho liên kết dòng phiếu nhập qua cặp `ReferenceType = GoodsReceiptDetail` và `ReferenceId`. Không thay đổi schema để bổ sung khóa.

## File Backend

- Thêm [ReceivingContracts.cs](../CommercialWms.Api/Contracts/Receiving/ReceivingContracts.cs).
- Thêm [ReceivingController.cs](../CommercialWms.Api/Controllers/ReceivingController.cs).
- Thêm [IReceivingService.cs](../CommercialWms.Api/Services/IReceivingService.cs).
- Thêm [ReceivingService.cs](../CommercialWms.Api/Services/ReceivingService.cs).
- Sửa [Program.cs](../CommercialWms.Api/Program.cs) để đăng ký service.

## File Frontend

- Thêm [ReceivingPage.tsx](../CommercialWms.Web/src/pages/ReceivingPage.tsx).
- Thêm [receivingApi.ts](../CommercialWms.Web/src/services/receivingApi.ts).
- Sửa [contracts.ts](../CommercialWms.Web/src/types/contracts.ts) để khai báo DTO.
- Sửa [AppRouter.tsx](../CommercialWms.Web/src/routes/AppRouter.tsx) và [AppLayout.tsx](../CommercialWms.Web/src/layouts/AppLayout.tsx) để bật route/menu.
- Sửa [index.css](../CommercialWms.Web/src/index.css) cho tab, bảng và biểu mẫu nhập hàng.

## Kiểm tra

- `dotnet build CommercialWms.Api/CommercialWms.Api.csproj --no-restore`: thành công, 0 cảnh báo, 0 lỗi.
- `npm run lint`: thành công.
- `npm run build`: thành công.
- Runtime: OpenAPI, GET đơn mua, GET phiếu nhập trả `200`; trạng thái lọc không hỗ trợ trả `400`; ID không tồn tại trả `404`.
- Payload không hợp lệ gửi tới API tạo đơn mua, tạo phiếu nhập và xác nhận phiếu đều trả `400`. Các request này không đi tới thao tác ghi dữ liệu.
- Frontend `/receiving` được mở bằng browser, tải được 5 đơn mua và 5 phiếu nhập từ database; trang chi tiết phiếu và thông tin kiểm tra chất lượng được mở thành công.
- `SELECT` sau kiểm tra xác nhận database đích là `WareHouse`, có 5 đơn mua, 5 phiếu nhập và 24 giao dịch tồn kho. Không chạy lệnh ghi dữ liệu.

## Giới hạn và việc còn lại

- Dữ liệu hiện có đều ở trạng thái đơn mua `Received` và phiếu nhập `Accepted`; không có đơn mua mở phù hợp để kiểm thử luồng nhận hàng thành công. Vì vậy không tạo đơn/phiếu thử trên database dùng chung. Cần một database kiểm thử được phê duyệt hoặc bộ dữ liệu kiểm thử riêng để chạy end-to-end qua ghi nhận, kiểm tra chất lượng và cộng tồn.
- Ứng dụng chưa có đăng nhập/phân quyền; endpoint duyệt đơn mua hiện chưa giới hạn theo vai trò.
- Chưa có automated integration test cho giao dịch nhận hàng; hiện mới xác minh build, endpoint đọc, phản hồi lỗi đầu vào và UI với dữ liệu sẵn có.
- Chuyển đổi số lượng theo đơn vị cơ sở được làm tròn về 3 chữ số thập phân (`MidpointRounding.AwayFromZero`) theo precision tồn kho hiện có. Cần PO xác nhận chính sách làm tròn nghiệp vụ.
- Có cảnh báo HTTPS redirection không xác định cổng HTTPS khi chạy profile HTTP. API vẫn khởi động và phản hồi trên `http://localhost:5206`; cảnh báo này không ảnh hưởng kết quả kiểm tra.

Giai đoạn tồn kho, xuất hàng, báo cáo và cài đặt chưa được triển khai trong báo cáo này.
