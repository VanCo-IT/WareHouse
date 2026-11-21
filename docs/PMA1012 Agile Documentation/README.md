# PMA1012 Agile Documentation

## Mục tiêu

Bộ này là khung hồ sơ assignment, không phải báo cáo tiến độ hoặc nghiệm thu Sprint. Mọi dữ liệu nhóm, meeting, Trello, test run, defect và kết quả cần được nhập từ bằng chứng thật. Dùng `[CẦN ĐIỀN]` nếu chưa được cung cấp.

Nguồn chính là [đề PMA1012](../../Assignment_PMA1012%20-%20Qu%E1%BA%A3n%20l%C3%BD%20d%E1%BB%B1%20%C3%A1n%20v%E1%BB%9Bi%20Agile.pdf) và các file trong [Mau tai lieu](../../Mau%20tai%20lieu). Assignment yêu cầu nhóm 4–6 người; nếu tài liệu Scrum tham khảo ghi khác, ưu tiên đề bài và xác nhận với giảng viên khi cần.

## Hướng dẫn sử dụng

1. Điền [Traceability Y1–Y5](00-Y1-Y5-Traceability.md) trước, chỉ đánh dấu hoàn thành khi có bằng chứng.
2. Điền tên thành viên, vai trò, Product Goal, Sprint, capacity và Definition of Done sau khi nhóm thống nhất.
3. Dùng các bản sao làm việc trong [Templates](Templates/); mẫu gốc trong `Mau tai lieu` không bị chỉnh sửa.
4. Các workbook backlog/test/defect đã được làm sạch dữ liệu ví dụ cũ nhưng giữ sheet, hàng tiêu đề và định dạng. Sheet RULE trong backlog và Rules trong workbook họp giữ nguyên như nội dung tham khảo của template, không tự động áp dụng quy tắc cũ.
5. Mẫu biên bản trong thư mục giảng viên là bảng nhận xét/đánh giá thành viên, không phải biên bản đầy đủ theo agenda. [Khung biên bản bổ sung](08-sprint-meeting-minutes-framework.md) chỉ là tài liệu hỗ trợ, không thay thế mẫu giảng viên.
6. Mẫu `.mpp` và `.vsd` chưa được đọc đầy đủ do không có Microsoft Project/Visio. Không tạo bản sao hoặc tuyên bố đã đáp ứng định dạng đó. Y1 formal deliverable đang chờ bản PDF/ảnh xuất hoặc môi trường có ứng dụng tương ứng.
7. Các test case trong [Y3](03-Y3-test-case-framework.md) là kế hoạch chưa chạy; không điền Pass/Fail khi chưa thực hiện. Không thử thao tác ghi lên database dùng chung `WareHouse`.
8. Dùng [Checklist đóng gói](09-submission-checklist.md) trước khi tạo ZIP. Tên đăng nhập sinh viên, người nộp và cấu trúc ZIP là `[CẦN ĐIỀN]`.

## Danh sách tài liệu

- [00 — Traceability Y1–Y5](00-Y1-Y5-Traceability.md)
- [01 — Y1 phân rã chức năng và phân công (draft nội dung, không thay thế mẫu)](01-Y1-function-decomposition-draft.md)
- [02 — Y2 User Story và backlog](02-Y2-user-story-framework.md)
- [03 — Y3 Test Case](03-Y3-test-case-framework.md)
- [04 — Y3 Trello tracking](04-Y3-trello-tracking-framework.md)
- [05 — Y4 mã nguồn và hướng dẫn chạy](05-Y4-source-and-run-guide.md)
- [06 — Y5 dàn ý slide](06-Y5-defense-slides-outline.md)
- [07 — Khung Burndown](07-burndown-framework.md) và [CSV dữ liệu trống](07-burndown-data-template.csv)
- [08 — Khung biên bản Sprint bổ sung](08-sprint-meeting-minutes-framework.md)
- [09 — Checklist đóng gói](09-submission-checklist.md)

## Trạng thái khi tạo khung

- Không có roster, vai trò, Sprint goal, lịch Sprint, Trello URL/history, meeting thật, test result hoặc defect WMS được cung cấp.
- API, frontend và database là bằng chứng sản phẩm; không chứng minh code đã được hoàn thành trong một Sprint cụ thể.
- Không chạy build/test/API ghi trong giai đoạn tạo tài liệu này.
- Không thay đổi code, database, migration, connection string hay dữ liệu.
