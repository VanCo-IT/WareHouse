# Y1 — Functional Decomposition

## 1. Phạm vi và trạng thái

Sơ đồ phân rã Commercial WMS theo chức năng nghiệp vụ chính. Phân loại dựa trên backend, frontend và tài liệu WMS hiện có. Sự tồn tại của bảng/Entity hoặc demo data không được xem là chức năng đã triển khai.

Sơ đồ chỉnh sửa được: [Y1-Functional-Decomposition.mmd](Y1-Functional-Decomposition.mmd).

| Nhãn | Ý nghĩa |
|---|---|
| **Có API + UI** | Backend endpoint và frontend screen/flow tương ứng có trong source |
| **Chỉ schema/database** | Entity/table/relationship tồn tại; chưa có API/UI nghiệp vụ |
| **Chưa triển khai** | Chưa thấy chức năng/API/UI phù hợp trong phạm vi source |
| **Công việc dự kiến** | Hạng mục có thể phát triển tiếp; PO/nhóm phải xác nhận |

## 2. Cây chức năng

### Cấp 0 — Commercial WMS

### 1. Danh mục sản phẩm

- 1.1 Quản lý danh mục sản phẩm — **Có API + UI**
- 1.2 Quản lý thương hiệu — **Có API + UI**
- 1.3 Quản lý sản phẩm — **Có API + UI**
- 1.4 Đơn vị sản phẩm, chính sách kho và yêu cầu bảo quản — **Chỉ schema/database**
- 1.5 Bổ sung UI/API cho 1.4 — **Công việc dự kiến**

### 2. Nhà cung cấp và nhà sản xuất

- 2.1 Xem/tạo/sửa và đổi trạng thái nhà cung cấp — **Có API + UI**
- 2.2 Quản lý liên hệ nhà cung cấp — **Có API + UI**
- 2.3 Liên kết nhà cung cấp–sản phẩm–nhà sản xuất — **Có API + UI**
- 2.4 Hồ sơ nhà sản xuất, chứng từ nhà cung cấp/nhà sản xuất và an toàn thực phẩm — **Chỉ schema/database**
- 2.5 Bổ sung API/UI quản lý chứng từ và nhà sản xuất — **Công việc dự kiến**

### 3. Kho và vị trí

- 3.1 Quản lý kho — **Chỉ schema/database**
- 3.2 Cấu trúc vị trí: Zone → Aisle → Rack → Shelf → Bin — **Chỉ schema/database**
- 3.3 Phân công nhân viên vào kho — **Chỉ schema/database**
- 3.4 Nhật ký nhiệt độ kho — **Chỉ schema/database**
- 3.5 API/UI quản lý kho và vị trí — **Công việc dự kiến**; menu Kho hàng hiện báo đang phát triển

### 4. Mua hàng và nhập kho

- 4.1 Purchase Orders và chi tiết — **Chỉ schema/database**
- 4.2 Goods Receipts và chi tiết nhận hàng — **Chỉ schema/database**
- 4.3 Lịch sử giá mua — **Chỉ schema/database**
- 4.4 API/UI lập đơn và nhận hàng — **Công việc dự kiến**; menu Nhập hàng hiện báo chưa có API

### 5. Kiểm tra chất lượng

- 5.1 Quality Inspections và chi tiết kiểm tra — **Chỉ schema/database**
- 5.2 API/UI ghi nhận kết quả và xử lý kiểm định — **Công việc dự kiến**

### 6. Tồn kho và lô hàng

- 6.1 Inventory Lots, hạn dùng và nguồn gốc — **Chỉ schema/database**
- 6.2 Inventory Balances theo Bin — **Chỉ schema/database**
- 6.3 Inventory Transactions — **Chỉ schema/database**
- 6.4 Stock Reservations — **Chỉ schema/database**
- 6.5 API/UI truy vấn và quản lý tồn kho — **Công việc dự kiến**; menu Tồn kho hiện báo đang phát triển

### 7. Điều chuyển và điều chỉnh tồn kho

- 7.1 Stock Transfers và chi tiết — **Chỉ schema/database**
- 7.2 Stock Adjustments và chi tiết — **Chỉ schema/database**
- 7.3 API/UI thực hiện, phê duyệt và theo dõi điều chuyển/điều chỉnh — **Công việc dự kiến**

### 8. Bán hàng và xuất kho

- 8.1 Sales Orders và chi tiết — **Chỉ schema/database**
- 8.2 Deliveries và chi tiết giao hàng — **Chỉ schema/database**
- 8.3 Customer Returns và chi tiết — **Chỉ schema/database**
- 8.4 Payments và chi tiết phân bổ — **Chỉ schema/database**
- 8.5 API/UI xử lý đơn bán, picking, giao hàng và trả hàng — **Công việc dự kiến**; menu Xuất hàng hiện báo đang phát triển

### 9. Khách hàng và nhân viên

- 9.1 Customer Types, Customers và Customer Addresses — **Chỉ schema/database**
- 9.2 Employees và Warehouse Employees — **Chỉ schema/database**
- 9.3 API/UI quản lý khách hàng/nhân viên — **Công việc dự kiến**

### 10. Báo cáo, kiểm kê và hệ thống

- 10.1 Dashboard số lượng catalog — **Có API + UI**
- 10.2 Stock Counts và chi tiết kiểm kê — **Chỉ schema/database**
- 10.3 System Settings, Audit Logs và temperature logs — **Chỉ schema/database**
- 10.4 Báo cáo vận hành kho — **Chưa triển khai API/UI báo cáo**
- 10.5 Menu Báo cáo/Cài đặt hiện chỉ là placeholder — **Chưa triển khai nghiệp vụ**

## 3. Nguồn đối chiếu

- [Bảng trạng thái triển khai WMS](../../WMS%20Business%20Documentation/06-implementation-traceability.md)
- [Bảng API và data map](../../WMS%20Business%20Documentation/07-api-and-data-map.md)
- Backend: [Controllers](../../../CommercialWms.Api/Controllers), [Entities](../../../CommercialWms.Api/Entities), [DbContext](../../../CommercialWms.Api/Data/CommercialWmsDbContext.cs)
- Frontend: [AppRouter.tsx](../../../CommercialWms.Web/src/routes/AppRouter.tsx)

Các nhánh “Công việc dự kiến” cần Văn Có (PO) và nhóm xác nhận trước khi được đưa vào Product/Sprint Backlog.

Danh sách work item và mức ưu tiên đề xuất: [Y1-Work-Breakdown.md](Y1-Work-Breakdown.md). Bảng phân công thành viên: [Y1-Team-Assignment.md](Y1-Team-Assignment.md).
