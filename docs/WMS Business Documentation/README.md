# WMS Business Documentation

## Mục đích và nguồn

Bộ này mô tả hệ thống Commercial WMS theo hiện trạng quan sát được, không xem sự tồn tại của Entity/bảng là bằng chứng một nghiệp vụ đã được triển khai.

Nguồn đối chiếu:

- Backend: [project overview](../../README.md), [CommercialWms.Api](../../CommercialWms.Api), [Controllers](../../CommercialWms.Api/Controllers), [Contracts](../../CommercialWms.Api/Contracts), [Entities](../../CommercialWms.Api/Entities), [CommercialWmsDbContext](../../CommercialWms.Api/Data/CommercialWmsDbContext.cs).
- Frontend: [CommercialWms.Web](../../CommercialWms.Web), [AppRouter](../../CommercialWms.Web/src/routes/AppRouter.tsx), [catalogApi](../../CommercialWms.Web/src/services/catalogApi.ts).
- Database/setup: [DATABASE_SETUP.md](../../DATABASE_SETUP.md), [InitialCreate migration](../../CommercialWms.Api/Data/Migrations/20261116075251_InitialCreate.cs), [additional demo seed](../../CommercialWms.Api/Data/Seeds/CommercialWmsAdditionalDemoData.sql).
- SQL Server: `localhost\SQL2022`, database `WareHouse`. Thông tin schema/dòng dữ liệu là snapshot chỉ đọc; các dòng demo được đánh dấu `DEMO` trong dữ liệu và không phải dữ liệu vận hành thật.

## Nhãn trạng thái

- **API + UI:** có route backend và màn hình frontend tương ứng. Không mặc định rằng mọi thao tác ghi đã được kiểm thử.
- **Schema/database only:** có bảng/Entity và có thể có dữ liệu mẫu, nhưng không có API/UI để thực hiện nghiệp vụ.
- **Not implemented:** chưa thấy đủ schema/API/UI để khẳng định chức năng.
- **Future proposal:** ý tưởng cần Product Owner/nghiệp vụ xác nhận, không phải cam kết hoặc trạng thái hiện tại.

## Nội dung

1. [Tổng quan và phạm vi](01-overview-and-scope.md)
2. [Actor và Use Case](02-actors-and-use-cases.md)
3. [Mô tả Use Case chi tiết](03-detailed-use-cases.md)
4. [Quy tắc nghiệp vụ đã xác minh](04-business-rules.md)
5. [Functional Decomposition](05-functional-decomposition.md)
6. [Bảng trạng thái triển khai](06-implementation-traceability.md)
7. [API và bản đồ dữ liệu](07-api-and-data-map.md)
8. [Nguồn sơ đồ chỉnh sửa được](diagrams/)

## Lưu ý nghiệm thu

- API hiện mới cung cấp các nhóm catalog/supplier; không mô tả đơn hàng, nhập kho, xuất kho, thanh toán hoặc kiểm kê là workflow đã vận hành.
- Database snapshot có 56 bảng nghiệp vụ và 464 dòng, không có bảng nghiệp vụ nào trống. Dữ liệu demo có thể làm các bảng trông như đã được sử dụng nhưng không chứng minh quy trình thật.
- Snapshot ghi nhận 101 FK bật, 25 CHECK, 55 DEFAULT, 40 unique index (bao gồm PK index) và 2 computed columns.
- Không có xác thực/phân quyền được cấu hình cho các API controller hiện tại. Vai trò chi tiết phải được nghiệp vụ xác nhận.
- Các thao tác ghi của API không được thử trong giai đoạn lập tài liệu này.
