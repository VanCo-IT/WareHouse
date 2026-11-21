# Y3 — Test Case framework

**Tất cả case dưới đây có trạng thái `NOT RUN`.** Đây là kế hoạch kiểm thử, không phải kết quả. Workbook giảng viên đã làm sạch được giữ tại [Test report — working copy](Templates/Test%20report.xls). Không thử POST/PUT/PATCH/DELETE trên database `WareHouse` dùng chung.

| ID | Phạm vi | Tiền điều kiện / bước chính | Kết quả mong đợi theo code | Actual / tester / ngày / trạng thái |
|---|---|---|---|---|
| TC-GET-001 | Categories list | GET `/api/Categories` | 200 và `CategoryResponse[]` | `[CẦN ĐIỀN]` / NOT RUN |
| TC-GET-002 | Brands list | GET `/api/Brands` | 200 và `BrandResponse[]` | `[CẦN ĐIỀN]` / NOT RUN |
| TC-GET-003 | Products list | GET `/api/Products` | 200 và `ProductResponse[]` | `[CẦN ĐIỀN]` / NOT RUN |
| TC-GET-004 | Suppliers list | GET `/api/Suppliers` | 200 và `SupplierResponse[]` | `[CẦN ĐIỀN]` / NOT RUN |
| TC-GET-005 | Product detail not found | GET `/api/Products/{id}` với ID không tồn tại | 404 | `[CẦN ĐIỀN]` / NOT RUN |
| TC-VAL-001 | Create Product thiếu SKU/tên bắt buộc | Chạy chỉ trên DB test cô lập; gửi POST thiếu trường | API validation trả 400 | `[CẦN ĐIỀN]` / NOT RUN |
| TC-VAL-002 | Create Product với Category không tồn tại | DB test cô lập; POST CategoryId sai | 400 invalid reference | `[CẦN ĐIỀN]` / NOT RUN |
| TC-DUP-001 | Duplicate SKU | DB test cô lập có fixture; POST SKU đã dùng | 409 Conflict | `[CẦN ĐIỀN]` / NOT RUN |
| TC-DUP-002 | Duplicate barcode | DB test cô lập có fixture; POST barcode đã dùng | 409 Conflict | `[CẦN ĐIỀN]` / NOT RUN |
| TC-DEL-001 | Xóa Product đang được tham chiếu | DB test cô lập với fixture quan hệ | 409; không xóa dữ liệu tham chiếu | `[CẦN ĐIỀN]` / NOT RUN |
| TC-SUP-001 | Supplier–Product duplicate link | DB test cô lập có fixture | 409 Conflict | `[CẦN ĐIỀN]` / NOT RUN |
| TC-SUP-002 | Xóa link có purchase history | DB test cô lập có fixture lịch sử | 409; association còn nguyên | `[CẦN ĐIỀN]` / NOT RUN |

## Khi thực hiện

Ghi request/response đã loại bỏ dữ liệu nhạy cảm, môi trường, phiên bản build, tester, ngày/giờ, kết quả thực tế, ảnh evidence và defect ID liên quan. Dùng database riêng được phép reset hoặc fixture được phê duyệt; không chạy trên `A`, `CommercialWMS` hoặc `WareHouse` dùng chung.
