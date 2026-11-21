# Y1 — Team Assignment

Bảng phân công dự kiến cho đúng sáu thành viên và vai trò do người dùng cung cấp. Trách nhiệm/đầu ra là đề xuất gắn với vai trò, không xác nhận thành viên đã thực hiện hoặc có năng lực ngoài vai trò đã nêu.

Danh sách work item được phân công tham chiếu tại [Y1-Work-Breakdown.md](Y1-Work-Breakdown.md); phân rã chức năng tại [Y1-Functional-Decomposition.md](Y1-Functional-Decomposition.md).

| Thành viên | Vai trò (giữ nguyên) | Trách nhiệm dự kiến trong Y1 | Đầu ra dự kiến |
|---|---|---|---|
| Văn Có | Product Owner (PO) | Xác nhận phạm vi WMS, ý nghĩa ưu tiên P1/P2, chấp thuận cách ghi trạng thái và các mục còn thiếu | Xác nhận phạm vi/ưu tiên Y1; quyết định nội dung nào là dự kiến |
| Thế Anh | Scrum Master | Điều phối buổi review Y1 khi nhóm tổ chức; nhắc nhóm ghi nhận quyết định và việc cần theo dõi | Kế hoạch/ghi nhận điều phối Y1 nếu buổi review thực sự diễn ra |
| Hoàng | Developer + Tester | Đối chiếu chức năng có API/UI với source; góp ý phân rã catalog/supplier và xác định các nhánh chưa có API/UI | Bằng chứng source tham chiếu và góp ý sơ đồ; không ghi nhận test result nếu chưa chạy |
| Nam | Database Developer | Đối chiếu nhóm Entity/bảng và quan hệ; giúp phân biệt schema/database với chức năng API/UI | Bảng ánh xạ nghiệp vụ–schema tham khảo; không sửa database/schema |
| Giang | Viết báo cáo | Tổng hợp Functional Decomposition, Work Breakdown và Team Assignment; giữ liên kết tới nguồn, ghi `[CẦN ĐIỀN]` cho thông tin thiếu | Bộ tài liệu Y1 đã biên tập để nhóm review |
| Khải | Thuyết trình | Chuẩn bị cách trình bày cây chức năng và trạng thái hiện tại sau khi PO/nhóm duyệt; ghi rõ giới hạn API/UI | Dàn ý trình bày Y1; không khẳng định đã thuyết trình hoặc tạo slide hoàn chỉnh |

## Ánh xạ trách nhiệm theo work item

| Work item | Đầu mối dự kiến | Phối hợp | Ghi chú |
|---|---|---|---|
| Y1-W01, Y1-W13 | Văn Có (PO) | Thế Anh, Giang | Chờ PO/nhóm xác nhận |
| Y1-W02, Y1-W03 | Hoàng | Văn Có, Giang | Phân loại theo source API/UI |
| Y1-W04–Y1-W10 | Nam | Hoàng, Giang | Phân loại schema/database và trạng thái API/UI |
| Y1-W11 | Giang | Hoàng, Nam, Văn Có | Sơ đồ cần review trước khi dùng trình bày |
| Y1-W12 | Thế Anh | Cả nhóm | Chỉ điều phối xác nhận phân công; không tự ghi nhận tiến độ |
| Y1-W13 — diễn giải sơ đồ khi trình bày | Khải | Cả nhóm | Chỉ chuẩn bị sau khi nội dung được PO/nhóm xác nhận |

## Thông tin chưa có

- Mã sinh viên/định danh nộp bài của từng thành viên: `[CẦN ĐIỀN]`.
- Người review/approver chính thức ngoài các trách nhiệm dự kiến trên: `[CẦN ĐIỀN]`.
- Ngày, hình thức review, due date, estimate và trạng thái thực tế: `[CẦN ĐIỀN]`.

Không có năng lực bổ sung, số giờ, kết quả hoặc cuộc họp nào được giả định trong bảng này.
