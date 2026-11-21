# Y1 — Tình trạng mẫu giảng viên và công cụ

## Kết quả kiểm tra công cụ

| Công cụ/định dạng | Kiểm tra hiện tại | Kết quả |
|---|---|---|
| Microsoft Visio / COM `Visio.Application` | Kiểm tra ProgID và command | Không có |
| Microsoft Project / COM `MSProject.Application` | Kiểm tra ProgID và command | Không có |
| Microsoft Excel / COM `Excel.Application` | Kiểm tra ProgID | Có; dùng để tạo bảng phân công `.xlsx` |
| Mermaid CLI / `mmdc` | Kiểm tra command | Không có |
| Graphviz / `dot` và PlantUML | Kiểm tra command | Không có |

Hai mẫu gốc có trong [Mau tai lieu](../../../Mau%20tai%20lieu):

- [Phan ra chuc nang phan mem.vsd](../../../Mau%20tai%20lieu/Phan%20ra%20chuc%20nang%20phan%20mem.vsd)
- [Phan cong cong viec.mpp](../../../Mau%20tai%20lieu/Phan%20cong%20cong%20viec.mpp)

Các file `.vsd` và `.mpp` không được mở/đọc đầy đủ qua trình xem file; không có ứng dụng tương ứng để xác minh layout, trường dữ liệu và tạo bản sao chỉnh sửa. Không ghi vào hai file gốc, cũng không tạo file giả mang đuôi `.vsd`/`.mpp`.

## Sản phẩm trung gian

- [Y1-Functional-Decomposition.mmd](Y1-Functional-Decomposition.mmd): sơ đồ Mermaid có thể chỉnh sửa bằng văn bản. Đã kiểm tra cấu trúc flowchart, node và edge; chưa render thành ảnh vì không có renderer.
- [Y1-Work-Assignment.xlsx](Y1-Work-Assignment.xlsx): workbook Excel chỉnh sửa được với Work Breakdown, Team Assignment và Notes. Không phải bản sao của mẫu Project; giảng viên cần xác nhận có thể dùng thay thế khi nộp.
- [Y1-Functional-Decomposition.md](Y1-Functional-Decomposition.md), [Y1-Work-Breakdown.md](Y1-Work-Breakdown.md), [Y1-Team-Assignment.md](Y1-Team-Assignment.md): tài liệu hỗ trợ nội dung, không tự nhận là biểu mẫu giảng viên.

## Cần xác nhận

- Giảng viên có chấp nhận sơ đồ Mermaid và bảng Excel thay cho mẫu `.vsd`/`.mpp` không? `[CẦN XÁC NHẬN]`
- Nếu phải dùng đúng định dạng gốc, cần máy có Visio/Project hoặc bản PDF/ảnh/Excel xuất từ hai mẫu để xác định layout và tiếp tục.
- P1/P2 trong workbook là mức ưu tiên hồ sơ Y1 do nhóm đề xuất; PO cần xác nhận trước khi coi là thống nhất.
