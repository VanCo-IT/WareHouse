# 2. Actor và Use Case

## 2.1 Actor

| Actor | Trạng thái | Căn cứ / việc cần xác nhận |
|---|---|---|
| Người dùng nội bộ chưa xác định | Actor tổng quát cho các API hiện tại | API không yêu cầu đăng nhập; `[CẦN XÁC NHẬN]` ai được dùng từng chức năng |
| Nhân viên quản trị catalog | Actor đề xuất | Có UI/API Categories, Brands, Products; vai trò/ủy quyền chưa được triển khai |
| Nhân viên quản lý nhà cung cấp | Actor đề xuất | Có UI/API Supplier, Contacts, Supplier Products; vai trò/ủy quyền chưa được triển khai |
| Nhân viên kho, mua hàng, kiểm định, bán hàng, kế toán | Actor tương lai `[CẦN XÁC NHẬN]` | Database có các miền dữ liệu tương ứng, nhưng chưa có API/UI workflow |
| Nhà cung cấp/khách hàng bên ngoài | Chưa được hỗ trợ qua portal/API | Không có endpoint đăng nhập hoặc portal bên ngoài trong phạm vi đã kiểm tra |

UI có thông tin người dùng dạng placeholder; không dùng placeholder đó làm tên actor hoặc bằng chứng có xác thực.

## 2.2 Use Case hiện có trong code/UI

| ID | Use Case | Actor | Phạm vi | Trạng thái |
|---|---|---|---|---|
| UC-CAT-01 | Xem/tìm danh mục sản phẩm | Người dùng nội bộ | GET categories; tìm kiếm ở UI | API + UI |
| UC-CAT-02 | Tạo/cập nhật/xóa danh mục | Người dùng nội bộ | POST/PUT/DELETE; kiểm tra mã, parent, tham chiếu | API + UI; ghi chưa kiểm thử trong giai đoạn này |
| UC-BRD-01 | Xem/tìm thương hiệu | Người dùng nội bộ | GET brands | API + UI |
| UC-BRD-02 | Tạo/cập nhật/xóa thương hiệu | Người dùng nội bộ | Country hợp lệ, duy nhất theo tên và Country, kiểm tra sản phẩm tham chiếu | API + UI; ghi chưa kiểm thử trong giai đoạn này |
| UC-PRD-01 | Xem danh sách/chi tiết sản phẩm | Người dùng nội bộ | GET products và GET theo id | API + UI |
| UC-PRD-02 | Tạo/cập nhật/xóa sản phẩm | Người dùng nội bộ | SKU/barcode, Category/Brand, tham chiếu khi xóa | API + UI; ghi chưa kiểm thử trong giai đoạn này |
| UC-SUP-01 | Xem/tạo/cập nhật/kích hoạt nhà cung cấp | Người dùng nội bộ | GET/POST/PUT/PATCH; không có DELETE Supplier | API + UI |
| UC-SUP-02 | Quản lý liên hệ nhà cung cấp | Người dùng nội bộ | GET/POST/PUT/DELETE contacts | API + UI |
| UC-SUP-03 | Quản lý liên kết Supplier–Product–Manufacturer | Người dùng nội bộ | GET/POST/PUT/DELETE association | API + UI |
| UC-OPS-01 | Nhập hàng, xuất hàng, chuyển kho, kiểm kê, thanh toán, trả hàng | Vai trò nghiệp vụ `[CẦN XÁC NHẬN]` | Có schema/dữ liệu demo nhưng không có API/UI workflow | Schema/database only |

Use Case Diagram có mã nguồn chỉnh sửa tại [use-case.puml](diagrams/use-case.puml). Các actor nghiệp vụ đề xuất cần được xác nhận trước khi đưa vào hồ sơ chính thức.
