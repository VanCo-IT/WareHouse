# Y1 — Phân rã chức năng và phân công

Thư mục này chỉ phục vụ Y1. Nội dung lấy từ source Commercial WMS, bộ WMS documentation và assignment PMA1012; phân biệt chức năng hiện có với schema-only và công việc dự kiến.

## Tài liệu

1. [Y1-Functional-Decomposition.md](Y1-Functional-Decomposition.md) — phân rã cấp hệ thống → nhóm chức năng → chức năng con.
2. [Y1-Functional-Decomposition.mmd](Y1-Functional-Decomposition.mmd) — sơ đồ Mermaid chỉnh sửa được.
3. [Y1-Work-Breakdown.md](Y1-Work-Breakdown.md) — danh sách công việc, đầu ra và ưu tiên đề xuất.
4. [Y1-Team-Assignment.md](Y1-Team-Assignment.md) — phân công dự kiến cho đúng sáu thành viên/role do nhóm cung cấp.
5. [Y1-Work-Assignment.xlsx](Y1-Work-Assignment.xlsx) — bảng Excel làm việc, có sheet Work Breakdown, Team Assignment và Notes; không phải bản sao `.mpp`.
6. [Y1-Template-Status.md](Y1-Template-Status.md) — tình trạng đọc/chỉnh sửa mẫu gốc và giới hạn công cụ.

## Cách sử dụng

- Dùng `Có API + UI` chỉ khi thấy endpoint backend và màn hình frontend tương ứng.
- Dùng `Chỉ schema/database` khi hiện có Entity/DB table nhưng chưa có API/UI nghiệp vụ.
- `Chưa triển khai` nghĩa là chưa thấy chức năng hoạt động trong phạm vi source đã kiểm tra.
- `Công việc dự kiến` là đề xuất cho Y1, chưa phải cam kết, Sprint plan hay kết quả đã làm.
- P1/P2 là mức ưu tiên **đề xuất cho việc hoàn thiện hồ sơ Y1**, không phải Product Backlog priority. Văn Có (PO) cần xác nhận.
- Không có estimate, ngày hoàn thành hoặc trạng thái tiến độ nào được tự tạo. Nhóm cập nhật sau khi thống nhất; nội dung chưa biết ghi `[CẦN ĐIỀN]`.

## Nguồn và giới hạn

- Assignment: [Assignment PMA1012](../../../Assignment_PMA1012%20-%20Qu%E1%BA%A3n%20l%C3%BD%20d%E1%BB%B1%20%C3%A1n%20v%E1%BB%9Bi%20Agile.pdf).
- Backend/API: [CommercialWms.Api](../../../CommercialWms.Api), [Controllers](../../../CommercialWms.Api/Controllers), [CommercialWmsDbContext](../../../CommercialWms.Api/Data/CommercialWmsDbContext.cs).
- Frontend routes: [AppRouter.tsx](../../../CommercialWms.Web/src/routes/AppRouter.tsx).
- WMS status evidence: [implementation traceability](../../WMS%20Business%20Documentation/06-implementation-traceability.md), [functional decomposition](../../WMS%20Business%20Documentation/05-functional-decomposition.md).
- Mẫu tham khảo gốc: [Mau tai lieu](../../../Mau%20tai%20lieu).

Mẫu `Phan ra chuc nang phan mem.vsd` và `Phan cong cong viec.mpp` chưa đọc được đầy đủ vì môi trường không có Visio/Project. Các file trong thư mục này không được tuyên bố là bản sao hoặc đáp ứng định dạng của hai mẫu đó. Nội dung Y1 vẫn cần được đối chiếu với giảng viên/nhóm nếu định dạng mẫu là bắt buộc.

Sơ đồ Mermaid và workbook Excel là **bản làm việc trung gian**. Chưa tạo SVG vì máy không có Mermaid renderer; chưa tạo file `.vsd`/`.mpp`. Xem [Y1-Template-Status.md](Y1-Template-Status.md) trước khi chọn file giao nộp.

Không có sửa code/database/migration, gọi API ghi, commit hoặc push trong quá trình lập bộ Y1.
