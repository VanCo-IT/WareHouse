# Bộ tài liệu Commercial WMS và PMA1012

Hai bộ tài liệu được tách riêng để không nhầm tài liệu mô tả sản phẩm với tài liệu chứng minh quá trình làm assignment:

- [WMS Business Documentation](WMS%20Business%20Documentation/README.md): mô tả phạm vi, code/API, dữ liệu, quy tắc và trạng thái triển khai.
- [PMA1012 Agile Documentation](PMA1012%20Agile%20Documentation/README.md): khung hồ sơ assignment, traceability Y1–Y5, backlog/test/meeting/burndown và đóng gói.

## Quy ước điền thông tin

- Thay `[CẦN ĐIỀN]` bằng thông tin được nhóm xác nhận.
- Không chuyển trạng thái sang Done/Pass/Completed nếu chưa có bằng chứng thực tế.
- Tách rõ nội dung đã kiểm chứng từ code/database với đề xuất nghiệp vụ.
- Chỉ điền kết quả thử nghiệm sau khi thực hiện và ghi môi trường, ngày, người thực hiện.
- Mẫu giảng viên trong `Mau tai lieu` là bản gốc. Chỉ chỉnh bản sao trong `PMA1012 Agile Documentation/Templates`.
- Không đưa connection string cá nhân, mật khẩu, token hoặc dữ liệu cá nhân vào tài liệu.

## Phạm vi kiểm tra

Nguồn WMS được đối chiếu với backend, frontend và trạng thái đọc của database `WareHouse` vào ngày 2026-11-18. Không có thao tác ghi database, API ghi, sửa code hoặc migration trong quá trình lập bộ tài liệu.

Các bản sao Excel được làm trống dữ liệu ví dụ cũ nhưng giữ sheet, tiêu đề và định dạng. Mẫu `.mpp` và `.vsd` chưa được đọc đầy đủ vì môi trường không có Microsoft Project/Visio; bộ PMA ghi rõ các đầu ra phụ thuộc hai mẫu này còn chờ xử lý.
