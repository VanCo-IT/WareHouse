# 3. Mô tả Use Case chi tiết

Các luồng dưới đây mô tả hành vi được đọc từ controller/DTO/UI. Đây không phải biên bản kiểm thử HTTP mới. Với các thao tác ghi, chỉ chạy kiểm thử trong môi trường được phê duyệt, không dùng dữ liệu thật tùy tiện.

## UC-PRD-01 — Xem danh sách và chi tiết sản phẩm

- **Actor:** Người dùng nội bộ (tên/vai trò thực tế `[CẦN XÁC NHẬN]`).
- **Tiền điều kiện:** API hoạt động và kết nối database cấu hình.
- **Luồng chính:** Người dùng mở trang Products → UI gọi `GET /api/Products` → backend đọc không tracking, sắp theo `ProductId` → trả `ProductResponse[]` → UI hiển thị.
- **Chi tiết:** UI chọn một sản phẩm → gọi `GET /api/Products/{id}` → trả `ProductResponse`.
- **Ngoại lệ:** ID không tồn tại trả 404; lỗi mạng/API được UI hiển thị lỗi.
- **Hậu điều kiện:** Không thay đổi dữ liệu.
- **DTO response:** `ProductId`, `SKU`, `ProductName`, `CategoryId`, `BrandId`, `Barcode`, `Description`, `IsPerishable`, `IsActive`, `CreatedAt`.

## UC-PRD-02 — Tạo sản phẩm

- **Tiền điều kiện:** Category tồn tại; nếu có Brand thì Brand phải tồn tại và đang active.
- **Luồng chính:** Người dùng nhập form → UI kiểm tra trường bắt buộc và kiểu dữ liệu → gửi `ProductCreateRequest` tới `POST /api/Products` → controller kiểm tra Category/Brand, SKU và barcode → lưu qua EF Core → trả 201 với `ProductResponse`.
- **Ngoại lệ:** DTO không hợp lệ hoặc tham chiếu sai trả 400; SKU/barcode trùng trả 409.
- **Hậu điều kiện:** Product mới được lưu nếu request thành công.
- **Lưu ý:** Không gửi request tạo mới trong giai đoạn tạo bộ tài liệu.

## UC-PRD-03 — Cập nhật hoặc xóa sản phẩm

- **Cập nhật:** `PUT /api/Products/{id}`; 404 nếu không có sản phẩm; xác thực tham chiếu và mã duy nhất; 400 hoặc 409 khi lỗi; 200 khi thành công.
- **Xóa:** `DELETE /api/Products/{id}`; 404 nếu không tìm thấy; 409 nếu còn dữ liệu nghiệp vụ tham chiếu; 204 nếu xóa thành công.
- **Rủi ro dữ liệu:** Không thực hiện xóa thật trên dữ liệu đang dùng. Case kiểm thử xóa phải dùng bản ghi cô lập trong database thử nghiệm và được phê duyệt.

## UC-CAT-02 — Quản lý danh mục

- **Luồng chính:** UI gọi GET danh sách/chi tiết hoặc POST/PUT/DELETE `/api/Categories`.
- **Quy tắc:** `CategoryCode` duy nhất; `ParentCategoryId` nếu có phải tồn tại và không được bằng ID của chính danh mục.
- **Ngoại lệ:** Parent không hợp lệ trả 400; trùng code hoặc xóa danh mục đang có Product/danh mục con trả 409; ID không tồn tại trả 404.
- **Giới hạn đã biết:** Code kiểm tra parent tự tham chiếu trực tiếp, chưa thấy kiểm tra chu trình nhiều cấp.

## UC-BRD-02 — Quản lý thương hiệu

- **Luồng chính:** UI gọi GET/POST/PUT/DELETE `/api/Brands`.
- **Quy tắc:** `CountryId` phải tồn tại; tên thương hiệu kết hợp Country phải duy nhất.
- **Ngoại lệ:** Country không tồn tại trả 400; trùng hoặc xóa Brand đang được Product tham chiếu trả 409; ID không tồn tại trả 404.

## UC-SUP-02 / UC-SUP-03 — Quản lý liên hệ và liên kết nhà cung cấp

- **Liên hệ:** `/api/suppliers/{supplierId}/contacts`; hỗ trợ GET/POST/PUT/DELETE. Supplier/contact không tồn tại trả 404.
- **Liên kết sản phẩm:** `/api/Suppliers/{id}/products`; hỗ trợ GET/POST/PUT/DELETE. Association gắn supplier, product và manufacturer; bộ ba duy nhất. Xóa association bị từ chối nếu có lịch sử mua hàng.
- **Lưu ý:** Country và Manufacturer không có endpoint tra cứu riêng trong API hiện có; UI có thể cần nhập ID hoặc cần API tra cứu được phê duyệt.

## Các luồng ngoài phạm vi API

Schema và demo data thể hiện các quan hệ mua hàng, nhận hàng, kiểm định, tồn kho, chuyển kho, bán hàng, giao hàng, trả hàng và thanh toán. Không có Use Case API tương ứng để xác nhận một luồng nghiệp vụ được người dùng thực hiện từ UI. Không chuyển nội dung seed thành mô tả chức năng đã hoàn thành.
