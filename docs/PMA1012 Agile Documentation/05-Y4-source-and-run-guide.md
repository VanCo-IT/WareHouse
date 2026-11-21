# Y4 — Danh mục mã nguồn và hướng dẫn chạy

## Danh mục sản phẩm có trong workspace

| Thành phần | Đường dẫn | Bằng chứng phạm vi |
|---|---|---|
| ASP.NET Core API | `CommercialWms.Api/` | .NET 10, EF Core 10.0.12, Controllers, Contracts, 56 Entities/configurations, migration |
| React frontend | `CommercialWms.Web/` | React 19, TypeScript, Vite 6, router, catalog pages |
| Tài liệu DB | `DATABASE_SETUP.md` | Target `WareHouse`, hướng dẫn migration/seed/API |
| Schema tham chiếu | `databaseA.sql` | Không phải script triển khai; không đưa vào gói bài nếu chưa được cho phép |

Build/restore gần nhất được ghi trong README dự án; **không chạy lại** trong giai đoạn soạn tài liệu. Nhóm cần lưu log/screenshot từ build thực tế được giao nộp.

## Cách chạy theo tài liệu dự án

### Backend

Yêu cầu .NET SDK 10.0.401 theo `global.json`; SQL Server và connection string cấu hình cục bộ.

```powershell
dotnet restore CommercialWms.Api\CommercialWms.Api.csproj
dotnet build CommercialWms.Api\CommercialWms.Api.csproj
dotnet run --project CommercialWms.Api\CommercialWms.Api.csproj --launch-profile http
```

HTTP mặc định theo README: `http://localhost:5206`; OpenAPI Development: `http://localhost:5206/openapi/v1.json`.

### Frontend

```powershell
Set-Location CommercialWms.Web
npm ci
npm run dev
```

Frontend dùng Vite proxy `/api` tới `http://localhost:5206`. Đảm bảo API đã chạy trước khi thử trang catalog.

### Cấu hình database

Tham khảo [DATABASE_SETUP.md](../../DATABASE_SETUP.md) và `CommercialWms.Api/appsettings.Example.json`. Giữ secrets và connection string cá nhân ngoài Git. Không chạy migration/seed trên database dùng chung nếu chưa xác nhận target và được phép.

## Kết quả demo cần nhóm ghi nhận

- Commit/source snapshot: `[CẦN ĐIỀN]`
- .NET restore/build log: `[CẦN ĐIỀN]`
- Frontend install/build log: `[CẦN ĐIỀN]`
- API/browser demo date, người trình bày và URL: `[CẦN ĐIỀN]`
- Giới hạn đã biết: phần lớn nghiệp vụ ngoài catalog/supplier chưa có API/UI.
