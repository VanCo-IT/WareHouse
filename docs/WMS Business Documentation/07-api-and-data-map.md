# 7. API và bản đồ dữ liệu

Base URL theo cấu hình tài liệu dự án: `http://localhost:5206`. Trong Development, OpenAPI được map tại `/openapi/v1.json`. Frontend dùng Vite proxy `/api` tới địa chỉ này.

## 7.1 Endpoint hiện có

| Nhóm | Method và route | Request | Response / ghi chú |
|---|---|---|---|
| Categories | GET `/api/Categories` | — | `CategoryResponse[]` |
| Categories | GET `/api/Categories/{id}` | — | `CategoryResponse`; 404 nếu không có |
| Categories | POST `/api/Categories` | `CategoryCreateRequest` | 201; 400 validation/reference; 409 trùng code |
| Categories | PUT `/api/Categories/{id}` | `CategoryUpdateRequest` | 200; 404/400/409 |
| Categories | DELETE `/api/Categories/{id}` | — | 204; 404/409 nếu đang được dùng |
| Brands | GET `/api/Brands`, GET `/api/Brands/{id}` | — | `BrandResponse[]` / `BrandResponse` |
| Brands | POST `/api/Brands` | `BrandCreateRequest` | 201; 400/409 |
| Brands | PUT `/api/Brands/{id}` | `BrandUpdateRequest` | 200; 404/400/409 |
| Brands | DELETE `/api/Brands/{id}` | — | 204; 404/409 nếu Product tham chiếu |
| Products | GET `/api/Products`, GET `/api/Products/{id}` | — | `ProductResponse[]` / `ProductResponse` |
| Products | POST `/api/Products` | `ProductCreateRequest` | 201; 400/409 |
| Products | PUT `/api/Products/{id}` | `ProductUpdateRequest` | 200; 404/400/409 |
| Products | DELETE `/api/Products/{id}` | — | 204; 404/409 nếu bản ghi được tham chiếu |
| Suppliers | GET `/api/Suppliers`, GET `/api/Suppliers/{id}` | — | `SupplierResponse[]` / `SupplierResponse` |
| Suppliers | POST `/api/Suppliers` | `SupplierCreateRequest` | 201; 400/409 |
| Suppliers | PUT `/api/Suppliers/{id}` | `SupplierUpdateRequest` | 200; 404/400/409 |
| Suppliers | PATCH `/api/Suppliers/{id}/status` | `SupplierStatusUpdateRequest` | Cập nhật `IsActive` |
| Supplier products | GET/POST `/api/Suppliers/{id}/products` | `SupplierProductCreateRequest` khi POST | List / 201; tham chiếu và trùng được kiểm tra |
| Supplier products | PUT/DELETE `/api/Suppliers/{id}/products/{productId}?manufacturerId={id}` | `SupplierProductUpdateRequest` khi PUT | PUT 200; DELETE 204; DELETE có thể trả 409 khi có lịch sử mua |
| Supplier contacts | GET/POST `/api/suppliers/{supplierId}/contacts` | `SupplierContactCreateRequest` khi POST | List / 201 |
| Supplier contacts | PUT/DELETE `/api/suppliers/{supplierId}/contacts/{contactId}` | `SupplierContactUpdateRequest` khi PUT | PUT 200; DELETE 204 |

Response DTO chính: Category gồm `CategoryId`, `CategoryCode`, `CategoryName`, `ParentCategoryId`, `IsActive`; Brand gồm `BrandId`, `BrandName`, `CountryId`, `IsActive`; Supplier gồm code/name/country và contact fields; Product gồm SKU/name/category/brand/barcode/description/perishable/active/createdAt. API trả DTO, không trả trực tiếp EF Entity.

Không có endpoint CRUD cho các nghiệp vụ nhận hàng, tồn kho, sales, delivery, returns, payments, stock counts, warehouses, countries hoặc manufacturers. Không tự tạo endpoint từ các bảng.

## 7.2 Sơ đồ dữ liệu

Sơ đồ ERD mức domain trong [erd-overview.mmd](diagrams/erd-overview.mmd) thể hiện quan hệ chọn lọc: catalog, supplier, customer/order, warehouse location, lot/balance và giao dịch. Nó không liệt kê tất cả cột/constraint và không thay thế migration hoặc schema SQL.

`StockAdjustmentDetails.DifferenceQuantity` và `StockCountDetails.DifferenceQuantity` là computed columns, không phải trường nhập tay.

## 7.3 Bằng chứng database

Metadata SQL Server được truy vấn chỉ đọc sau khi xác nhận `DB_NAME() = 'WareHouse'`. Snapshot: 56 bảng nghiệp vụ; 464 dòng; 101 FK bật; 25 CHECK; 55 DEFAULT; 40 unique index gồm PK index. Không thực hiện INSERT/UPDATE/DELETE/DDL trong lúc tạo tài liệu.

Kết quả GET smoke test trước đó trong README dự án ghi nhận Categories 5, Brands 5, Products 20, Suppliers 5. Các thao tác ghi chưa được kiểm thử lại trong giai đoạn này.
