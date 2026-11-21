# Commercial WMS Web

Frontend quản trị kho bằng React, TypeScript và Vite. Giao diện đọc dữ liệu thật từ CommercialWms.Api; không dùng dữ liệu mẫu thay cho API.

## Công nghệ

- React 19
- TypeScript 7
- Vite 6
- React Router 7
- npm

## Chạy local

Mở hai terminal tại thư mục workspace:

```powershell
Set-Location .\CommercialWms.Api
dotnet run --launch-profile http
```

```powershell
Set-Location .\CommercialWms.Web
npm install
npm run dev
```

Vite chuyển tiếp các request `/api` tới `http://localhost:5206`. Nếu API chạy ở địa chỉ khác, cập nhật `server.proxy['/api'].target` trong `vite.config.ts`. Backend cần được cấu hình kết nối SQL Server riêng; frontend không chứa connection string.

## Kiểm tra

```powershell
npm run lint
npm run build
```

## Màn hình và API

- Tổng quan: số lượng thực tế từ Categories, Brands, Products và Suppliers.
- Danh mục: xem, tìm kiếm, lọc, thêm, sửa, xóa.
- Thương hiệu: xem, tìm kiếm, lọc, thêm, sửa, xóa.
- Sản phẩm: xem, tìm kiếm, lọc theo danh mục/trạng thái, thêm, sửa, xóa.
- Nhà cung cấp: xem, tìm kiếm, lọc, thêm, sửa và kích hoạt/ngừng hoạt động; không có API xóa nhà cung cấp.
- Chi tiết nhà cung cấp: quản lý Supplier Contacts và Supplier Products theo các endpoint hiện có.
- Kho hàng, nhập hàng, tồn kho, xuất hàng, báo cáo và cài đặt: đang phát triển do chưa có API tương ứng.

API hiện chưa cung cấp endpoint tra cứu Countries hoặc Manufacturers. Vì vậy, các biểu mẫu liên quan yêu cầu nhập `CountryId` hoặc `ManufacturerId` đã tồn tại; không tự tạo danh sách giả.

Các thao tác ghi chỉ được gửi khi người dùng chủ động submit biểu mẫu hoặc xác nhận thao tác trong giao diện.
