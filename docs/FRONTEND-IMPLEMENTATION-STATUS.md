# Commercial WMS — Frontend Implementation Status

**Ngày rà soát:** 2026-11-19  
**Phạm vi:** Đọc cấu trúc React/TypeScript/Vite, routes, navigation, pages, API clients, shared UI/hooks và đối chiếu với các backend routes hiện có. Không sửa code/backend, không thay đổi cấu hình hoặc database, không gọi API ghi.

## 1. Tóm tắt

- Frontend dùng React `19.3.1`, TypeScript `~7.0.2`, Vite `6.4.6`, React Router `7.18.5`.
- Có 15 file trong `src/pages`; trong đó có 14 màn hình chức năng được route (một số màn hình danh mục dùng `ReferenceCatalogPage` dùng chung). Có thêm `/reports`, `/settings` và route wildcard nhưng chúng chỉ render placeholder.
- Có sáu API client: `apiClient`, `catalogApi`, `warehouseApi`, `receivingApi`, `inventoryApi`, `outboundApi`. API requests dùng `/api` qua Vite proxy tới `http://localhost:5206`.
- Các màn hình danh mục, kho, nhập hàng, tồn kho và xuất hàng đều có API client tương ứng. Chúng có trạng thái loading/error/empty ở các danh sách chính; một số lỗi tải dữ liệu phụ chưa có nút thử lại.
- Dashboard hiện là tổng quan **danh mục**, không phải tổng quan chỉ số vận hành kho.
- Khách hàng hiện chỉ là dữ liệu lựa chọn của đơn bán; chưa có màn hình quản lý khách hàng. Báo cáo và cài đặt là placeholder, chưa nối API.
- `npm run lint` đạt; `npm run build` đạt (TypeScript build và Vite production build). Không có script/test frontend được cấu hình.
- Đây là rà soát source và build; không khởi động lại ứng dụng hoặc thực hiện kiểm thử browser/API trong lượt này. Không kết luận thao tác ghi hoạt động chỉ từ việc có form và API client.

## 2. Cấu trúc frontend

```text
CommercialWms.Web/src/
  components/   Ui.tsx — DataTable, Modal, Error/Notice/Validation, badge, empty state
  hooks/        useApiQuery.ts
  layouts/      AppLayout.tsx — sidebar, navigation, user-chip tĩnh
  pages/        15 file TSX, gồm trang nghiệp vụ và trang danh mục dùng chung
  routes/       AppRouter.tsx
  services/     apiClient và 5 API service theo domain
  types/        contracts.ts — TypeScript DTO
  App.tsx
  main.tsx
  index.css
```

Scripts hiện có: `dev`, `build`, `lint`, `preview`; không có `test`. `vite.config.ts` cấu hình proxy development `/api` cố định tới `http://localhost:5206`; chưa thấy API base URL theo môi trường trong frontend.

## 3. Routing và navigation

Các màn hình có page component riêng:

| Route | Màn hình | Hiện trạng |
|---|---|---|
| `/` | Tổng quan | Có; đọc số lượng Products, Categories, Brands, Suppliers từ API. |
| `/products` | Sản phẩm | Có; danh sách/lọc, thêm, sửa, xóa. |
| `/categories` | Danh mục sản phẩm | Có; danh sách/tìm/lọc, thêm, sửa, xóa, chọn danh mục cha. |
| `/brands` | Thương hiệu | Có; danh sách/tìm/lọc, thêm, sửa, xóa, chọn quốc gia. |
| `/suppliers` | Nhà cung cấp | Có; danh sách/tìm/lọc, thêm, sửa, bật/tắt trạng thái, xóa theo API hiện có. |
| `/suppliers/:id` | Chi tiết nhà cung cấp | Có; quản lý liên hệ và liên kết sản phẩm/nhà sản xuất. |
| `/countries` | Quốc gia | Có; dùng `ReferenceCatalogPage`, CRUD và chi tiết. |
| `/currencies` | Tiền tệ | Có; dùng `ReferenceCatalogPage`, CRUD và chi tiết. |
| `/units` | Đơn vị tính | Có; dùng `ReferenceCatalogPage`, CRUD và chi tiết. |
| `/manufacturers` | Nhà sản xuất | Có; dùng `ReferenceCatalogPage`, CRUD, chi tiết, quốc gia. |
| `/warehouses` | Kho và vị trí | Có; danh sách kho và cây Warehouse → Zone → Aisle → Rack → Shelf → Bin. |
| `/receiving` | Nhập hàng | Có; đơn mua, nhận hàng, kiểm nhận, chất lượng và xác nhận put-away. |
| `/inventory` | Tồn kho | Có; số dư, lịch sử, điều chỉnh và chuyển vị trí/kho. |
| `/outbound` | Xuất hàng | Có; đơn bán, chọn phân bổ lô/vị trí, giữ chỗ, phiếu xuất và trạng thái giao hàng. |
| `/reports` | Báo cáo | **Placeholder “Đang phát triển”; chưa có API/report UI.** |
| `/settings` | Cài đặt | **Placeholder “Đang phát triển”; chưa có API/settings UI.** |
| `*` | Route không khớp | Dùng cùng placeholder chung, không có trang 404 riêng. |

Sidebar điều hướng tới các màn hình danh mục, kho và nghiệp vụ trên. Báo cáo/cài đặt có nhãn “Đang phát triển”. Không có mục khách hàng, nhân viên, kiểm kê, trả hàng hay thanh toán. Chi tiết nhà cung cấp chỉ truy cập qua nút từ danh sách.

## 4. Đối chiếu UI với backend

| Module | UI và API integration hiện tại | Còn thiếu / giới hạn |
|---|---|---|
| Dashboard | `DashboardPage` gọi GET Products, Categories, Brands, Suppliers; có loading/error/retry. | Chỉ có số lượng catalog; chưa có tồn kho, nhập/xuất, đơn hàng hay cảnh báo vận hành. Không có dashboard API tổng hợp. |
| Sản phẩm / danh mục / thương hiệu | Trang CRUD tương ứng dùng `catalogApi`; danh sách có tìm/lọc, form validation, thông báo lỗi. Dropdown danh mục, thương hiệu, đơn vị, quốc gia lấy từ API. | Chưa có automated frontend tests; thao tác CRUD chưa được browser-test trong lượt rà soát này. |
| Nhà cung cấp | Danh sách và chi tiết kết nối API. Có CRUD nhà cung cấp theo endpoint, trạng thái, contacts và Supplier–Product–Manufacturer association. | Không phải màn hình quản lý customer; chưa có API/UI khách hàng độc lập. |
| Quốc gia / tiền tệ / đơn vị / nhà sản xuất | Dùng API CRUD trong `catalogApi`; các trang quốc gia/tiền tệ/đơn vị/nhà sản xuất dùng component chung. | Chưa có UI exchange rates, conversion/product units hoặc manufacturer documents. |
| Kho và vị trí | `WarehousesPage` dùng `warehouseApi` cho danh sách, cấu trúc và các hành động CRUD cấp kho/khu vực/dãy/kệ/kệ con/vị trí; có tìm/lọc và cây cấu trúc. | Không có Floor trong schema/API nên không có UI Floor. Không có automated frontend test cho thao tác cây/xóa tham chiếu. |
| Nhập hàng | `ReceivingPage` dùng `receivingApi` và catalog/warehouse APIs: đơn mua, gửi duyệt/duyệt/hủy, tạo phiếu, kiểm nhận, kiểm tra chất lượng, chọn lô/vị trí và xác nhận nhập. | Danh sách hiện tìm kiếm nhưng chưa thấy đầy đủ các filter status/supplier/warehouse/date mà API hỗ trợ. Một số lỗi tải dữ liệu tham chiếu chỉ hiện thông báo, không có retry. Nhân viên tạo/nhận/kiểm định đang gửi `null`; chưa có chọn người thực hiện. |
| Tồn kho / điều chỉnh / chuyển | `InventoryPage` gọi API tồn và transaction history, lọc sản phẩm/kho/vị trí/loại/ngày, phân trang, điều chỉnh và chuyển tồn. | UI ghi “Schema hiện tại không có mức tồn…”, nhưng schema có `ProductWarehousePolicies` với Min/Max/Safety; thiếu API/UI chính sách tồn. Đây là thông báo UI không chính xác về schema. Lỗi tải products/warehouses không có nút retry. |
| Đơn bán / phân bổ / lấy hàng / giao hàng | `OutboundPage` dùng `outboundApi`: danh sách/chi tiết đơn và giao hàng, tạo đơn, xem khả dụng, chọn lô/vị trí, xác nhận/giữ chỗ, chuẩn bị soạn, xác nhận xuất, giao hàng và hủy. | Customer/employee/address chỉ lấy từ API options; chưa có CRUD UI. Người xác nhận xuất đang gửi employee ID `null`. Không có browser test luồng ghi trong audit này. |
| Khách hàng | Có dropdown dữ liệu customer và địa chỉ trong UI outbound, lấy từ `GET /api/outbound/options`. | Không có route/page/API service quản lý khách hàng, loại khách hàng, địa chỉ; do đó không thể tạo/sửa customer từ frontend. |
| Báo cáo | Sidebar và `/reports` route tồn tại. | Route chỉ là placeholder; không có report API/client/charts/export. Không có UI nào chưa nối API ngoài placeholder này. |
| Cài đặt | Sidebar và `/settings` route tồn tại. | Route chỉ là placeholder; chưa có UI/API cài đặt. |
| Khác | Component user-chip hiển thị “Quản trị viên / Tài khoản demo”. | Đây là nội dung tĩnh, không có đăng nhập hoặc thông tin người dùng/phân quyền được tích hợp vào UI. Không có trang kiểm kê, trả hàng, thanh toán, audit, nhiệt độ, ngưỡng tồn hoặc exchange rates. |

## 5. Loading, error, empty và validation

### Đã có

- `DataTable` có hiển thị loading và empty state. Hầu hết danh sách chính truyền thông điệp empty cụ thể.
- `useApiQuery` tải GET theo route, hỗ trợ reload/retry và tránh cập nhật state sau khi component unmount.
- `apiClient` chuyển lỗi HTTP/problem-details và validation errors thành thông báo dùng được trong UI.
- Các form chính có validation phía client, trạng thái “Đang lưu…”, hiển thị lỗi API và khóa nút submit trong lúc lưu.
- Các thao tác xóa/hủy có bước xác nhận bằng `window.confirm`; một số thao tác thành công có notice có thể đóng.
- Loading/error được thể hiện riêng cho dữ liệu chi tiết hoặc lookup ở các luồng nhận hàng, chuyển kho, phân bổ xuất.

### Cần cải thiện

- Một số lỗi lookup không có nút retry: `ReferenceCatalogPage` khi tải quốc gia cho form nhà sản xuất; `ReceivingPage` khi tải các dữ liệu tham chiếu; `InventoryPage` khi tải products/warehouses.
- `useApiQuery` không hủy request mạng khi đổi route/filter nhanh; chỉ bỏ qua kết quả đến sau khi cleanup.
- Chưa có test UI tự động cho loading/error/empty, validation, điều hướng hoặc các luồng nhiều bước.
- Chưa xác minh keyboard/focus management, modal focus trap/escape, responsive trên thiết bị mục tiêu và accessibility end-to-end.
- UI lọc/phân trang không đồng đều: API nhận nhiều filter hơn phần nhập hàng đang cung cấp; dashboard không có filter.

## 6. Trang có nhưng chưa kết nối API

- `/reports`: placeholder, chưa có report API.
- `/settings`: placeholder, chưa có settings API.
- Wildcard `*`: placeholder dùng chung, không phải trang nghiệp vụ và không có 404 chuyên biệt.
- User chip: thông tin tĩnh, không gọi API identity.

Dashboard **có kết nối API**, nhưng chỉ tổng hợp danh mục; không phải UI-only. Khách hàng **không có trang UI**, dù outbound dùng danh sách khách hàng do API options trả về.

## 7. Danh sách UI còn thiếu

### Thiếu màn hình nghiệp vụ, backend hiện cũng chưa có API chuyên biệt

- Quản lý khách hàng, loại khách hàng, địa chỉ.
- Quản lý nhân viên và phân công nhân viên–kho.
- Báo cáo và xuất dữ liệu.
- Cài đặt hệ thống.
- Kiểm kê tồn kho.
- Trả hàng/kiểm tra hàng trả.
- Thanh toán/hoàn tiền.
- Theo dõi nhiệt độ kho, nhật ký kiểm toán.
- Quản lý tỷ giá, Product Units/conversion, chính sách Min/Max/Safety, yêu cầu bảo quản và tài liệu nhà sản xuất.

Với các module này, chỉ làm frontend sẽ không tạo được workflow thật khi backend chưa có endpoint tương ứng. Không dùng dữ liệu giả hoặc chỉ số hardcode để lấp chỗ trống.

### UI đã có nhưng cần bổ sung để demo liền mạch

- Dashboard nghiệp vụ kho dựa trên số liệu thật, sau khi thống nhất chỉ số và API nguồn.
- Customer master UI phụ thuộc Customer API; hiện outbound chỉ dùng lookup.
- Tồn kho thấp/cảnh báo dựa trên `ProductWarehousePolicies`; cần API/UI chính sách trước khi hiển thị ngưỡng.
- Filter nhập hàng tương ứng với các filter backend có sẵn.
- Lựa chọn/ghi nhận nhân viên thực hiện trong các thao tác receiving/shipment, phụ thuộc cơ chế identity/employee và quy tắc quyền.

## 8. Tài liệu và điểm cần lưu ý

- [CommercialWms.Web README](../CommercialWms.Web/README.md) còn ghi kho, nhập, tồn và xuất “đang phát triển do chưa có API”; điều này đã lỗi thời so với routes, pages, API clients hiện tại.
- [InventoryPage.tsx](../CommercialWms.Web/src/pages/InventoryPage.tsx) mô tả schema không có ngưỡng tồn; câu này không khớp với bảng `ProductWarehousePolicies` trong schema. Chức năng quản lý ngưỡng vẫn chưa được frontend/backend expose.
- Form UI/API client có nghĩa là code integration đã được viết, không chứng minh luồng ghi đã chạy thành công với dữ liệu SQL Server. Trong lượt này chỉ lint/build, không gửi request API ghi.

## 9. Thứ tự đề xuất để hoàn thiện bản demo

1. **Dọn thông tin demo sai lệch:** cập nhật README frontend; sửa trạng thái mô tả ngưỡng tồn trong inventory để phản ánh đúng schema và việc API/UI chưa có. Không tạo số liệu mẫu.
2. **Kiểm thử demo các luồng đang có:** chạy API + frontend trên môi trường được phép; xác nhận GET và tương tác UI. Thử POST/PUT chỉ với database integration test đã được PO duyệt, không dùng shared `WareHouse`.
3. **Hoàn thiện trải nghiệm hiện có:** bổ sung retry cho lookup lỗi, bộ lọc nhập hàng khớp API, và thông điệp rõ khi danh sách khách hàng rỗng nên không thể tạo đơn.
4. **Customer master:** ưu tiên tiếp theo nếu demo cần tạo customer mới; cần backend API khách hàng/địa chỉ trước UI CRUD. Nếu chỉ cần chọn customer đã có, giữ UI outbound hiện tại và xác minh lookup data.
5. **Dashboard kho và báo cáo:** chốt chỉ số cần demo, bổ sung API tổng hợp dựa trên database, sau đó mới làm dashboard/report UI. Không tổng hợp số liệu giả ở frontend.
6. **Settings và các nghiệp vụ còn lại:** chỉ xây dựng khi có yêu cầu nghiệp vụ/API tương ứng; stock count, return, payment và temperature cần thiết kế và kiểm thử workflow trước.

## 10. Kiểm tra đã chạy

```text
npm run lint  — passed
npm run build — passed (tsc -b và Vite production build)
```

Không có test frontend được chạy vì `package.json` chưa định nghĩa script/test framework. Không sửa source/config, không sửa backend/database và không commit/push trong quá trình rà soát.
