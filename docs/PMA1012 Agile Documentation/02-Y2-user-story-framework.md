# Y2 — User Story và backlog

## Bộ tài liệu Y2

- [Y2 README](Y2/README.md) — hướng dẫn sử dụng và các quyết định còn thiếu.
- [Product/Release/Sprint Backlog — workbook theo mẫu giảng viên](Y2/Y2-Product-Release-Sprint-Backlog.xlsx) — bản làm việc, giữ các sheet/cột của mẫu.
- [User Stories và Backlogs chi tiết](Y2/Y2-User-Stories-and-Backlogs.md) — 19 story, Acceptance Criteria, liên kết Y1, nhóm Release và Sprint 1 đề xuất.

Mẫu gốc trong `Mau tai lieu` không bị chỉnh sửa. Ưu tiên workbook theo mẫu; Markdown là tài liệu hỗ trợ giải thích, không thay thế workbook.

## Quy tắc sử dụng

- `US-01`–`US-06` có API/UI trong source; điều đó không chứng minh thao tác ghi đã được kiểm thử hoặc hoàn tất trong một Sprint.
- `US-07`–`US-19` là đề xuất dựa trên schema/database; chưa có API/UI nghiệp vụ theo tài liệu WMS.
- Vai trò chưa được xác nhận ghi `[CẦN XÁC NHẬN]`.
- Priority và Business Value trong backlog là thứ tự/đánh giá đề xuất, chưa được PO phê duyệt.
- Chỉ `US-01`–`US-03` được chọn vào Sprint 1 candidate; đây không phải cam kết Sprint đã diễn ra. Không có ngày, estimate, state thực tế hoặc kết quả test được tạo.

## Tóm tắt Product Backlog

| ID | Phạm vi | Hiện trạng |
|---|---|---|
| US-01–US-03 | Categories, Brands, Products | Có API + UI |
| US-04–US-06 | Suppliers, contacts, Supplier Products | Có API + UI |
| US-07–US-10 | Kho/vị trí, mua hàng/nhập kho, kiểm định, tồn kho/lô | Chỉ schema/database |
| US-11–US-16 | Điều chuyển/điều chỉnh, khách hàng, đơn bán, giao hàng, trả hàng, thanh toán | Chỉ schema/database |
| US-17–US-19 | Báo cáo, kiểm kê, cài đặt/audit/nhiệt độ | Dashboard catalog hiện có; các nghiệp vụ còn lại chỉ schema/database hoặc chưa có API/UI |

Chi tiết đầy đủ từng User Story, Acceptance Criteria, đề xuất Release/Sprint và người phụ trách task nằm trong [Y2 backlog chi tiết](Y2/Y2-User-Stories-and-Backlogs.md).

## Thông tin backlog cần Product Owner/nhóm xác nhận

- Product Goal, Release Goal và chấp thuận thứ tự ưu tiên/Business Value.
- Actor, quyền và Acceptance Criteria cuối cùng; các quy tắc nghiệp vụ chưa có bằng chứng phải được xác định trước khi phát triển.
- Sprint number, thời lượng/ngày, capacity, estimate unit, Definition of Ready/Done và quy ước State.
- Phê duyệt môi trường kiểm thử cô lập; không ghi dữ liệu test lên database dùng chung `WareHouse`.
- Người review/approver và cách lưu test evidence/defect thực tế.

Không xem backlog đề xuất hoặc Sprint 1 candidate là nội dung đã được nhóm cam kết hay hoàn thành.
