# 4. Quy tắc nghiệp vụ đã xác minh

Quy tắc được phân biệt theo nguồn: API/DTO, ràng buộc SQL Server hoặc vấn đề cần Product Owner xác nhận. CHECK constraint xác nhận giá trị/quan hệ dữ liệu, không đồng nghĩa đã có quy trình chuyển trạng thái.

## 4.1 Quy tắc được thực thi trong API/DTO

| ID | Quy tắc | Nguồn |
|---|---|---|
| BR-API-01 | Category code bắt buộc, dài 1–30 ký tự; tên bắt buộc, dài 1–100 ký tự; parent nếu có phải là ID dương và tồn tại | Category DTO + controller |
| BR-API-02 | Product SKU bắt buộc, dài 1–40; tên dài 1–180; Category ID dương; barcode tối đa 60; mô tả tối đa 500 | Product DTO |
| BR-API-03 | SKU và barcode không được trùng; Brand gắn khi tạo mới phải tồn tại và active; Category phải tồn tại | ProductsController |
| BR-API-04 | Không xóa Product còn tham chiếu từ các bảng nghiệp vụ | ProductsController |
| BR-API-05 | Brand name bắt buộc, tối đa 120; Country ID hợp lệ; cặp `(BrandName, CountryId)` không trùng | Brand DTO/controller/database |
| BR-API-06 | Không xóa Brand có Product tham chiếu | BrandsController |
| BR-API-07 | Supplier code 1–30; tên 1–180; Country ID dương; code duy nhất | Supplier DTO/controller |
| BR-API-08 | Supplier Contact name 1–120; các trường liên hệ có giới hạn độ dài theo DTO | Supplier Contact DTO |
| BR-API-09 | Supplier Product association duy nhất theo Supplier–Product–Manufacturer; không gỡ association khi có purchase history | SuppliersController/database |
| BR-API-10 | API xóa Supplier không tồn tại; API chỉ hỗ trợ tạo, cập nhật và bật/tắt trạng thái | SuppliersController |

## 4.2 Ràng buộc database đã xác nhận

Metadata `WareHouse` snapshot: 101 FK bật, 25 CHECK, 55 DEFAULT, 40 unique index (tính cả PK index). Ví dụ:

- `GoodsReceiptDetails`: `AcceptedQuantity + RejectedQuantity <= ReceivedQuantity`.
- `InventoryBalances`: `QuantityReserved <= QuantityOnHand`.
- `InventoryLots`, `FoodSafetyDocuments`, `ManufacturerDocuments`, `SupplierDocuments`: hạn dùng không trước ngày sản xuất/cấp phát khi các ngày tương ứng có giá trị.
- `ProductStorageRequirements`: khoảng nhiệt độ hợp lệ và `EffectiveTo >= EffectiveFrom`.
- `ProductWarehousePolicies`: MaxQuantity nếu có không nhỏ hơn MinQuantity.
- `StockTransfers`: kho nguồn khác kho đích; `StockTransferDetails.ReceivedQuantity <= RequestedQuantity`.
- Status CHECK giới hạn các chuỗi trạng thái hợp lệ trên PurchaseOrders, GoodsReceipts, SalesOrders, Deliveries, CustomerReturns, StockCounts, StockAdjustments, StockTransfers và StockReservations.
- `StockAdjustmentDetails.DifferenceQuantity` và `StockCountDetails.DifferenceQuantity` là computed: `ActualQuantity - SystemQuantity`.

## 4.3 Cần xác nhận nghiệp vụ

- Vai trò nào được tạo/sửa/xóa catalog và thay đổi trạng thái Supplier? `[CẦN ĐIỀN]`
- Cách xử lý chu trình danh mục cha–con nhiều cấp.
- Quy tắc phê duyệt và chuyển trạng thái giữa các trạng thái đã liệt kê.
- Chính sách lô, FEFO/FIFO, đặt trước, hoàn trả và hoàn tiền.
- Ai chịu trách nhiệm xác nhận dữ liệu demo trước khi trình diễn.

Không suy diễn các quy tắc này từ tên cột, Entity hoặc dữ liệu seed.
