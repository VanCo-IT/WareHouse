# Khung 2 — Quản lý kho và vị trí lưu trữ

## 1. Kết quả

Đã bổ sung API và giao diện quản lý kho theo cấu trúc vị trí đã có trong schema. Không thêm Entity hoặc bảng, không tạo/chạy migration, không sửa connection string và không ghi dữ liệu.

| Cấp | Backend | Giao diện |
|---|---|---|
| Kho | Danh sách, chi tiết, tạo, sửa, xóa; tìm kiếm, lọc trạng thái và phân trang | Danh sách, tìm kiếm/lọc, phân trang, biểu mẫu CRUD |
| Khu vực (`WarehouseZones`) | CRUD, lọc theo kho | Xem cây, thêm/sửa/xóa, chọn kho cha |
| Dãy (`WarehouseAisles`) | CRUD, lọc theo khu vực | Xem cây, thêm/sửa/xóa, chọn khu vực cha |
| Kệ (`WarehouseRacks`) | CRUD, lọc theo dãy | Xem cây, thêm/sửa/xóa, chọn dãy cha |
| Kệ con (`WarehouseShelves`) | CRUD, lọc theo kệ | Xem cây, thêm/sửa/xóa, chọn kệ cha |
| Vị trí (`WarehouseBins`) | CRUD, lọc theo kệ con và trạng thái | Xem cây, thêm/sửa/xóa, chọn kệ con cha |

Schema hiện có không có bảng hoặc Entity `Floor`. Do đó không thể triển khai tầng như một cấp độc lập mà không thay đổi thiết kế database. Giao diện và API bám theo các cấp thực tế: Kho → Khu vực → Dãy → Kệ → Kệ con → Vị trí.

## 2. API

Các controller dùng route `api/[controller]`:

- `GET/POST /api/Warehouses`; `GET/PUT/DELETE /api/Warehouses/{id}`; `GET /api/Warehouses/{id}/structure`
- `GET/POST /api/WarehouseZones`; `GET/PUT/DELETE /api/WarehouseZones/{id}`
- `GET/POST /api/WarehouseAisles`; `GET/PUT/DELETE /api/WarehouseAisles/{id}`
- `GET/POST /api/WarehouseRacks`; `GET/PUT/DELETE /api/WarehouseRacks/{id}`
- `GET/POST /api/WarehouseShelves`; `GET/PUT/DELETE /api/WarehouseShelves/{id}`
- `GET/POST /api/WarehouseBins`; `GET/PUT/DELETE /api/WarehouseBins/{id}`

Danh sách hỗ trợ tìm kiếm, lọc theo cấp cha và các trường trạng thái phù hợp, cùng phân trang API. API cấu trúc kho trả về cây vị trí cho kho được chọn. Các request/response dùng DTO riêng, không trả Entity EF Core trực tiếp.

POST trả `201 Created` kèm địa chỉ tài nguyên; PUT trả `200 OK`; DELETE trả `204 No Content`. ID không tồn tại trả `404`; dữ liệu đầu vào/quan hệ cha không hợp lệ trả lỗi `400`; mã bị trùng hoặc dữ liệu đang được tham chiếu trả lỗi xung đột.

## 3. Quy tắc và bảo vệ dữ liệu

- Mã kho duy nhất theo schema; mã ở từng cấp vị trí được kiểm tra theo cấp cha.
- Xác thực kho/khu vực/dãy/kệ/kệ con cha tồn tại trước khi lưu.
- Không xóa cấp cha khi còn cấp con.
- Không xóa kho đang có cấu trúc vị trí hoặc dữ liệu nghiệp vụ liên quan.
- Không xóa vị trí bin đang được tham chiếu bởi số dư tồn, giao dịch tồn, giữ chỗ, kiểm kê, điều chỉnh hoặc điều chuyển; không cho đổi cấp cha của bin khi còn các tham chiếu đó.
- Trường sức chứa bin được kiểm tra không âm và tối đa ba chữ số thập phân, theo độ chính xác của schema hiện có.
- Ứng dụng hiện chưa có cơ chế phân quyền theo vai trò; các endpoint mới không tự tạo hoặc giả lập phân quyền.

## 4. Giao diện

Trang **Kho và vị trí** hiển thị danh sách kho và cây cấu trúc của kho đang chọn. Có trạng thái tải, rỗng và lỗi; biểu mẫu tạo/sửa có validation; thao tác xóa yêu cầu xác nhận. Các lựa chọn cấp cha được lấy từ API, không hardcode kho hoặc danh sách vị trí.

## 5. File đã tạo

**Backend**

- `CommercialWms.Api/Contracts/Warehouses/`
- `CommercialWms.Api/Contracts/WarehouseZones/`
- `CommercialWms.Api/Contracts/WarehouseAisles/`
- `CommercialWms.Api/Contracts/WarehouseRacks/`
- `CommercialWms.Api/Contracts/WarehouseShelves/`
- `CommercialWms.Api/Contracts/WarehouseBins/`
- `CommercialWms.Api/Controllers/WarehouseControllerHelpers.cs`
- `CommercialWms.Api/Controllers/WarehousesController.cs`
- `CommercialWms.Api/Controllers/WarehouseZonesController.cs`
- `CommercialWms.Api/Controllers/WarehouseAislesController.cs`
- `CommercialWms.Api/Controllers/WarehouseRacksController.cs`
- `CommercialWms.Api/Controllers/WarehouseShelvesController.cs`
- `CommercialWms.Api/Controllers/WarehouseBinsController.cs`

**Frontend**

- `CommercialWms.Web/src/services/warehouseApi.ts`
- `CommercialWms.Web/src/pages/WarehousesPage.tsx`

## 6. File đã sửa

- `CommercialWms.Web/src/types/contracts.ts`
- `CommercialWms.Web/src/routes/AppRouter.tsx`
- `CommercialWms.Web/src/layouts/AppLayout.tsx`
- `CommercialWms.Web/src/index.css`

Không sửa Entity, DbContext, Fluent API Configuration, migration, dữ liệu hoặc connection string.

## 7. Kiểm tra

- `dotnet build CommercialWms.Api/CommercialWms.Api.csproj --no-restore`: thành công, 0 cảnh báo, 0 lỗi.
- `npm run lint`: thành công.
- `npm run build`: thành công; TypeScript và Vite production build hoàn tất.
- `git diff --check`: thành công.
- Mở trang `/warehouses` trên frontend: ứng dụng, route, sidebar và trạng thái lỗi API được render. Backend không chạy tại `http://localhost:5206` khi kiểm tra; GET `/api/Warehouses` không kết nối được và trang hiển thị lỗi request. Vì vậy chưa xác minh runtime với database hoặc hành vi CRUD qua API.
- Không gọi API ghi dữ liệu, không chạy migration và không thực hiện kiểm thử kết nối database.
- Không có test tự động cho các endpoint mới trong phạm vi kiểm tra này.

## 8. Cách kiểm tra tiếp

1. Cấu hình API trên máy thành viên trỏ tới database `WareHouse` được phép sử dụng và khởi động API.
2. Chạy frontend bằng `npm run dev`, mở `/warehouses`, kiểm tra danh sách và cây vị trí.
3. Dùng GET để kiểm tra danh sách, chi tiết và cấu trúc. Chỉ thử POST/PUT/DELETE trên môi trường và dữ liệu được phép; xác nhận các lỗi `400`, `404` và xung đột khi có dữ liệu tham chiếu.
4. PO/nhóm xác nhận có cần cấp Floor hay không. Nếu cần, cần một quyết định thay đổi schema riêng; Khung 2 hiện không thêm cấp đó.
