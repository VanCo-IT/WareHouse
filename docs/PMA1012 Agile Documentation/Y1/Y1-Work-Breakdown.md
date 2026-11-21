# Y1 — Work Breakdown

Danh sách dưới đây là công việc dự kiến để hoàn thiện hồ sơ Y1, không phải báo cáo công việc đã hoàn thành. Mức ưu tiên là đề xuất cho Y1; chưa có estimate, deadline hoặc trạng thái thực hiện.

Phân rã chức năng: [Y1-Functional-Decomposition.md](Y1-Functional-Decomposition.md). Đầu mối theo thành viên: [Y1-Team-Assignment.md](Y1-Team-Assignment.md).

### Quy ước ưu tiên

- **P1:** cần cho hồ sơ Y1 có thể đối chiếu và trình bày.
- **P2:** cần để phân rã đủ nghiệp vụ chính và nói rõ giới hạn triển khai.
- Các mức trên không phải cam kết Product Backlog; PO xác nhận trước khi nhóm dùng.

| ID | Mô tả công việc | Đầu ra dự kiến | Ưu tiên |
|---|---|---|---|
| Y1-W01 | Đối chiếu yêu cầu Y1 với đề PMA1012 và phạm vi Commercial WMS | Bảng nguồn, phạm vi và trạng thái | P1 |
| Y1-W02 | Phân rã Product Catalog: Categories, Brands, Products, units và policy | Nhánh catalog có nhãn API/UI hoặc schema-only | P1 |
| Y1-W03 | Phân rã Supplier/Manufacturer, contacts, product links và hồ sơ liên quan | Nhánh supplier/manufacturer có phân loại hiện trạng | P1 |
| Y1-W04 | Phân rã Warehouses, Zones, Aisles, Racks, Shelves, Bins và nhân viên kho | Nhánh kho/vị trí; không gán là API đã có | P2 |
| Y1-W05 | Phân rã Purchase Orders, Goods Receipts và lịch sử giá | Nhánh mua hàng/nhập kho; schema-only được ghi rõ | P2 |
| Y1-W06 | Phân rã Quality Inspections và Inspection Details | Nhánh kiểm tra chất lượng; schema-only được ghi rõ | P2 |
| Y1-W07 | Phân rã Inventory Lots, Balances, Transactions và Reservations | Nhánh tồn kho/lô; schema-only được ghi rõ | P2 |
| Y1-W08 | Phân rã Stock Transfers, Adjustments và Stock Counts | Nhánh điều chuyển/điều chỉnh/kiểm kê; phân biệt schema với API | P2 |
| Y1-W09 | Phân rã Sales Orders, Deliveries, Returns, Payments và Customers | Nhánh bán hàng/xuất kho/khách hàng; schema-only được ghi rõ | P2 |
| Y1-W10 | Phân rã Dashboard, báo cáo, System Settings, Audit Logs và nhiệt độ | Nhánh báo cáo/hệ thống; dashboard hiện có tách khỏi báo cáo chưa triển khai | P2 |
| Y1-W11 | Tạo và kiểm tra sơ đồ Mermaid tương ứng với cây chức năng | `Y1-Functional-Decomposition.mmd` | P1 |
| Y1-W12 | Lập bảng phân công theo sáu thành viên và vai trò được cung cấp | `Y1-Team-Assignment.md` chờ nhóm xác nhận | P1 |
| Y1-W13 | Rà soát thuật ngữ, nguồn bằng chứng, liên kết và nội dung cần PO xác nhận; chuẩn bị diễn giải sơ đồ sau khi được duyệt | Bộ Y1 sẵn sàng cho review nhóm; nội dung trình bày của Khải chỉ chuẩn bị sau khi PO/nhóm duyệt | P1 |

## Trường theo dõi sau khi nhóm thống nhất

| ID | Người phụ trách | Người review | Estimate/đơn vị | Due date | Trạng thái | Evidence |
|---|---|---|---|---|---|---|
| `[CẦN ĐIỀN]` | `[CẦN ĐIỀN]` | `[CẦN ĐIỀN]` | `[CẦN ĐIỀN]` | `[CẦN ĐIỀN]` | `[CẦN ĐIỀN]` | `[CẦN ĐIỀN]` |

Không điền số giờ, năng lực, tiến độ hoặc kết quả thay cho thành viên.
