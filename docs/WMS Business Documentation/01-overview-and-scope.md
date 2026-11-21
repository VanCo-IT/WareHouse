# 1. Tổng quan và phạm vi

## 1.1 Mục tiêu

Commercial WMS là ứng dụng quản trị kho thương mại sử dụng ASP.NET Core Web API, EF Core Code First và SQL Server. Backend có mô hình 56 bảng nghiệp vụ. Frontend React cung cấp dashboard và một số màn hình quản trị catalog/supplier.

Database đích được cấu hình trong môi trường đang kiểm kê là `WareHouse` trên `localhost\SQL2022`. Database `A` là nguồn tham chiếu chỉ đọc, không phải database ứng dụng.

## 1.2 Hiện trạng công nghệ

| Thành phần | Hiện trạng có bằng chứng |
|---|---|
| Backend | ASP.NET Core Web API, .NET 10, EF Core SQL Server 10.0.12 |
| Database | SQL Server; 56 Entity và 56 Fluent Configuration |
| Migration | `20261116075251_InitialCreate` |
| Frontend | React, TypeScript, Vite; proxy `/api` tới `http://localhost:5206` |
| OpenAPI | Được map khi chạy Development |
| Authentication/authorization | Chưa thấy cấu hình xác thực hoặc policy phân quyền gắn vào controller |
| Test project | Không tìm thấy project test .NET; frontend `package.json` không có script `test` |

## 1.3 Ranh giới chức năng

**Có API và giao diện:** danh mục sản phẩm, thương hiệu, sản phẩm, nhà cung cấp, thông tin liên hệ và liên kết nhà cung cấp–sản phẩm. Màn hình dashboard hiển thị số lượng catalog.

**Có schema/database nhưng chưa có API nghiệp vụ:** kho và vị trí, khách hàng, nhân viên, mua hàng, nhận hàng, kiểm định chất lượng, tồn kho/lô, điều chuyển/điều chỉnh, bán hàng, giao hàng, trả hàng, thanh toán, kiểm kê, nhiệt độ và audit.

**Chưa thể xác nhận:** quy trình người dùng thực tế, phân quyền theo vai trò, phê duyệt nghiệp vụ ngoài các trường trạng thái/ràng buộc SQL, báo cáo nghiệp vụ và tiêu chí vận hành production.

**Đề xuất tương lai:** chỉ xem xét sau khi Product Owner xác nhận vai trò, quy trình, chính sách tồn kho, trạng thái và tiêu chí nghiệm thu. Không coi các đề xuất trong bộ này là backlog đã được phê duyệt.

## 1.4 Bằng chứng dữ liệu

Snapshot đọc-only ngày 2026-11-18 ghi nhận 56 bảng nghiệp vụ, tổng 464 dòng, không có bảng nghiệp vụ nào trống. Các bảng catalog có 5 categories, 5 brands, 20 products và 5 suppliers. Dữ liệu workflow có mã `DEMO`; đây là dữ liệu mẫu, không phải giao dịch thực.

Các số liệu trên mô tả snapshot đã kiểm tra, không phải báo cáo realtime. Không chạy script seed trong giai đoạn tạo tài liệu.
