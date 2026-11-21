# Y2 — User Story và Backlog

## Sản phẩm chính

- [Y2-Product-Release-Sprint-Backlog.xlsx](Y2-Product-Release-Sprint-Backlog.xlsx) — bản làm việc theo các sheet và cột của mẫu Excel giảng viên, gồm `Product Backlog`, `Release Backlog`, `Sprint Backlog` và `RULE`.
- [Y2-User-Stories-and-Backlogs.md](Y2-User-Stories-and-Backlogs.md) — nội dung chi tiết, trạng thái triển khai, liên kết Y1, mapping Release/Sprint và các quyết định đang chờ.

Mẫu gốc trong `Mau tai lieu` không được chỉnh sửa. Workbook là bản làm việc dựa trên bản sao mẫu đã làm sạch trong `Templates`.

## Quy ước

- Có 19 User Story (`US-01` đến `US-19`); ID được dùng liên kết Product, Release và Sprint Backlog.
- Ưu tiên là thứ tự đề xuất (1 cao nhất); Business Value là đánh giá định tính đề xuất. Cả hai chưa được PO phê duyệt.
- Sheet `RULE` mô tả Business Value theo thang 1–100, trong khi các dòng ví dụ dùng High/Medium. Bản này dùng High/Medium như định dạng mẫu hiển thị và đánh dấu đề xuất; PO/giảng viên cần xác nhận có phải quy đổi sang thang điểm hay không.
- Các story nghiệp vụ ngoài Catalog/Supplier được đánh dấu schema/database-only; không tuyên bố đã có API/UI.
- Vai trò chưa được xác nhận ghi `[CẦN XÁC NHẬN]`; không dùng placeholder người dùng trên UI làm vai trò đã xác nhận.
- Chỉ `US-01` đến `US-03` được đưa vào Sprint 1 đề xuất, tập trung kiểm chứng chức năng Catalog hiện có. Sprint Goal, scope, state và mọi estimate chưa phải cam kết.
- Không có ngày phát hành, thời lượng Sprint, estimate, tiến độ hay kết quả kiểm thử được tự tạo.

## Cần nhóm/PO xác nhận

1. Vai trò người dùng, quyền và người nghiệm thu từng chức năng.
2. Thứ tự ưu tiên, Business Value, Product/Release Goal và ranh giới từng Release.
3. Sprint Goal, Sprint number/duration/dates, capacity và scope đã chọn.
4. Đơn vị estimate, Definition of Ready/Done và quy ước State.
5. Môi trường kiểm thử cô lập được phê duyệt; không ghi dữ liệu kiểm thử lên database dùng chung `WareHouse`.
6. Các tiêu chí nghiệp vụ còn mở trong từng story (trạng thái, chính sách tồn, giá, picking, trả hàng, thanh toán, báo cáo, ngưỡng nhiệt độ và audit).

