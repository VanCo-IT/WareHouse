# Báo cáo Khung 1 — Danh mục nền tảng WMS

## 1. Kết quả

Đã triển khai các module danh mục nền tảng trên schema hiện có. Không thêm Entity hoặc bảng, không tạo/chạy migration, không đổi connection string và không ghi dữ liệu vào database.

| Danh mục | API CRUD | Trang UI | Tìm kiếm/lọc |
|---|---|---|---|
| Quốc gia | Có | Có | Mã/tên, trạng thái |
| Tiền tệ | Có | Có | Mã/tên, trạng thái |
| Đơn vị tính | Có | Có | Mã/tên, trạng thái |
| Nhà sản xuất | Có | Có | Mã/tên, trạng thái |
| Nhóm hàng | Có sẵn, bổ sung lọc danh sách | Có sẵn | Mã/tên, trạng thái |
| Thương hiệu | Có sẵn, bổ sung lọc danh sách | Có sẵn | Tên, trạng thái |
| Sản phẩm | Có sẵn, bổ sung lọc danh sách | Có sẵn | SKU/tên/mã vạch, trạng thái |
| Nhà cung cấp | CRUD hoàn chỉnh, gồm bổ sung DELETE | Có, gồm xóa | Mã/tên/email/điện thoại, trạng thái |

Tất cả danh sách catalog hiện giữ nguyên response dạng mảng. Có thể dùng `search`, `isActive`, `page` và `pageSize`; khi phân trang được yêu cầu API trả thêm `X-Total-Count`, `X-Page`, `X-Page-Size` và `X-Total-Pages`.

## 2. API đã bổ sung hoặc mở rộng

Các API mới:

- `GET/POST /api/Countries`, `GET/PUT/DELETE /api/Countries/{id}`
- `GET/POST /api/Currencies`, `GET/PUT/DELETE /api/Currencies/{id}`
- `GET/POST /api/Units`, `GET/PUT/DELETE /api/Units/{id}`
- `GET/POST /api/Manufacturers`, `GET/PUT/DELETE /api/Manufacturers/{id}`
- `DELETE /api/Suppliers/{id}`

Các API danh sách `Categories`, `Brands`, `Products`, `Suppliers` được bổ sung tìm kiếm, lọc trạng thái và phân trang tùy chọn. Các API CRUD sẵn có tiếp tục dùng cùng route và DTO hiện hữu, ngoại trừ `Product` được mở rộng hợp đồng để nhận/trả `UnitId`.

POST trả `201 Created` kèm `Location`; PUT trả `200 OK`; DELETE trả `204 No Content`. Không tìm thấy trả `404`, dữ liệu tham chiếu không hợp lệ trả `400`, mã/record trùng hoặc record đang được tham chiếu trả `409`.

## 3. Quy tắc và bảo vệ dữ liệu

- Mã SKU tiếp tục được kiểm tra trùng; Category, Brand, Supplier và các danh mục mới xác thực dữ liệu theo độ dài, khóa duy nhất và khóa ngoại hiện có.
- Xóa Country, Currency, Unit, Manufacturer, Supplier và các catalog đã hỗ trợ xóa sẽ bị từ chối nếu còn dữ liệu tham chiếu.
- Product không có cột `UnitId` trong schema. Khi tạo, API tạo liên kết cơ sở trong `ProductUnits` với `IsBaseUnit = true` và `ConversionToBase = 1`, trong cùng transaction với Product. API không tự sửa schema.
- Khi cập nhật Product, không cho đổi đơn vị cơ sở hiện tại vì chưa có quy tắc chuyển đổi cho các đơn vị liên quan; UI hiển thị rõ hạn chế này.
- Supplier yêu cầu có ít nhất email hoặc số điện thoại. Đây là quy tắc kiểm tra được áp dụng tại API và form; PO cần xác nhận quy tắc này có phù hợp nghiệp vụ chính thức không.
- Brand, Supplier và Manufacturer dùng dropdown quốc gia lấy từ API thật; Product dùng dropdown Unit; liên kết Supplier–Product dùng dropdown Product và Manufacturer.

## 4. UI đã hoàn thành

Đã thêm các trang Quốc gia, Tiền tệ, Đơn vị tính và Nhà sản xuất vào router/sidebar. Các trang có danh sách, tìm kiếm, lọc trạng thái, phân trang phía trình duyệt, xem chi tiết, thêm, sửa, xóa có xác nhận, trạng thái tải/rỗng/lỗi và validation.

Các form Brand, Supplier, Product và Supplier Product được cập nhật để chọn các danh mục liên quan từ API thay vì nhập ID thủ công.

## 5. File đã tạo

**Backend**

- `CommercialWms.Api/Contracts/Common/CatalogListQuery.cs`
- `CommercialWms.Api/Contracts/Countries/*`
- `CommercialWms.Api/Contracts/Currencies/*`
- `CommercialWms.Api/Contracts/Units/*`
- `CommercialWms.Api/Contracts/Manufacturers/*`
- `CommercialWms.Api/Data/CatalogQueryExtensions.cs`
- `CommercialWms.Api/Controllers/CatalogControllerHelpers.cs`
- `CommercialWms.Api/Controllers/CountriesController.cs`
- `CommercialWms.Api/Controllers/CurrenciesController.cs`
- `CommercialWms.Api/Controllers/UnitsController.cs`
- `CommercialWms.Api/Controllers/ManufacturersController.cs`

**Frontend**

- `CommercialWms.Web/src/pages/ReferenceCatalogPage.tsx`
- `CommercialWms.Web/src/pages/CountriesPage.tsx`
- `CommercialWms.Web/src/pages/CurrenciesPage.tsx`
- `CommercialWms.Web/src/pages/UnitsPage.tsx`
- `CommercialWms.Web/src/pages/ManufacturersPage.tsx`

## 6. File đã sửa

- Backend: `CategoriesController.cs`, `BrandsController.cs`, `ProductsController.cs`, `SuppliersController.cs`; Product/Supplier request-response contracts.
- Frontend: `contracts.ts`, `catalogApi.ts`, `AppRouter.tsx`, `AppLayout.tsx`, `BrandsPage.tsx`, `ProductsPage.tsx`, `SuppliersPage.tsx`, `SupplierDetailPage.tsx`, `index.css`.

## 7. Kiểm tra

- `dotnet build CommercialWms.Api/CommercialWms.Api.csproj --no-restore`: thành công, 0 warning, 0 error.
- `npm run lint`: thành công, không còn cảnh báo.
- `npm run build`: thành công.
- `git diff --check`: thành công.
- Đã thử GET OpenAPI và `/api/Countries` tại `http://localhost:5206`; không kết nối được vì API chưa chạy. Do đó chưa xác minh runtime hoặc thao tác CRUD qua API thật. Không thực hiện POST/PUT/DELETE và không ghi database.
- Workspace không có test project .NET để chạy test tự động.

## 8. Cách kiểm thử thủ công và nội dung cần xác nhận

1. Cấu hình API trỏ đến database `WareHouse` trên máy thành viên và khởi động API; không chạy migration từ báo cáo này.
2. Khởi động frontend bằng `npm run dev`, kiểm tra từng trang danh mục và các dropdown.
3. Dùng GET để xác nhận danh sách và chi tiết. Chỉ thử POST/PUT/DELETE trên môi trường dữ liệu được phép dùng; kiểm tra `201`, `200`, `204`, `400`, `404` và `409`.
4. PO xác nhận quy tắc Supplier phải có email hoặc điện thoại và việc không cho đổi Unit cơ sở sau khi tạo Product.

Không có thay đổi cấu trúc database, migration, seed hoặc connection string trong Khung 1.
