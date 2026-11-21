# P0.1 — Kế hoạch môi trường SQL Server Integration Test

**Trạng thái:** Khảo sát và đề xuất; chờ Product Owner (PO) phê duyệt.  
**Mục tiêu database:** `WareHouse_IntegrationTest` trên `localhost\SQL2022`.  
**Phạm vi thực hiện trong tài liệu này:** Chỉ đọc source/configuration đã kiểm tra và lập kế hoạch. Không tạo database, chạy migration, ghi dữ liệu, sửa connection string, schema, migration hay nghiệp vụ.

## 1. Hiện trạng đã kiểm tra

- API dùng .NET 10 và EF Core SQL Server `10.0.12`; `dotnet-ef` local tool cũng là `10.0.12`.
- `Program.cs` đọc khóa `ConnectionStrings:CommercialWms`, đăng ký `CommercialWmsDbContext` với SQL Server, và **không** tự gọi `Migrate`, `EnsureCreated` hoặc `EnsureDeleted` khi khởi động.
- Có một migration C# chính: `20261116075251_InitialCreate`, cùng Designer và model snapshot. Không có migration bổ sung sau nó.
- Kiểm tra tĩnh migration cho thấy `Up` có 56 `CreateTable`, 49 `CreateIndex`, quan hệ/constraint, và 4 `InsertData` ổn định cho `Countries`, `Currencies`, `CustomerTypes`, `Units`. Các hàng này là dữ liệu tham chiếu; migration không phải script seed giao dịch demo.
- `Down` có 56 `DropTable`. Tuyệt đối không dùng `database update 0` hoặc lệnh rollback migration để dọn test.
- Đã có xác minh metadata trước đó trên `WareHouse`: 56 bảng nghiệp vụ và migration `InitialCreate` đã được ghi nhận. Không truy cập database `A` hoặc legacy `CommercialWMS`.
- `CommercialWms.Api.Tests` hiện chỉ có test service dùng EF Core InMemory (6 Inventory, 7 Outbound); chưa có test SQL Server hay test HTTP integration. Các test InMemory không xác minh transaction/constraint/locking thật của SQL Server.
- Có file cấu hình local bị Git ignore; không đọc hoặc sao chép nội dung file này. Không đưa connection string cá nhân vào tài liệu hoặc Git.

## 2. Kiến trúc đề xuất

```text
CommercialWms.Api.Tests
  ├─ Unit tests hiện tại: EF Core InMemory, không cần SQL Server
  └─ SQL integration tests (sau khi được duyệt)
       ├─ chỉ đọc WMS_TEST_CONNECTION_STRING
       ├─ từ chối chạy nếu server/database không khớp allowlist
       └─ kết nối WareHouse_IntegrationTest hoặc DB tạm theo từng lần chạy

SQL Server localhost\SQL2022
  ├─ WareHouse                    ứng dụng; giữ nguyên, không ghi trong test
  ├─ A                            tham chiếu; không kết nối
  ├─ CommercialWMS                legacy; không kết nối
  └─ WareHouse_IntegrationTest    môi trường test riêng, tạo bằng EF migration
```

Test phải tạo `CommercialWmsDbContext` với connection string dành riêng cho test. Không khởi động API bằng cấu hình kết nối ứng dụng mặc định trong test và không để test tự động fallback sang `WareHouse`.

**Chế độ khuyến nghị:** dùng một database dùng riêng cho test hoặc một database tạm có tên bắt đầu bằng `WareHouse_IntegrationTest_` cho mỗi lần chạy. Cách dùng database riêng theo lần chạy giúp tránh đụng dữ liệu giữa các test song song. Nếu nhóm chỉ dùng database cố định `WareHouse_IntegrationTest`, chạy tuần tự và thực hiện fixture cleanup có giới hạn rõ ràng.

## 3. Khả năng dựng schema từ đầu

Về mặt source, migration `InitialCreate.Up` được viết như migration baseline đầy đủ: tạo 56 bảng và các index, FK, constraint trong cùng migration. Vì vậy đây là cơ sở phù hợp để dựng schema trống bằng EF Core Code First; không cần chạy file SQL baseline.

Đây mới là **kết luận kiểm tra tĩnh**, không phải bằng chứng migration đã chạy thành công trên database trống. Chỉ xác nhận thực tế sau khi PO phê duyệt và migration được chạy trên đúng database test mới. Migration cũng chèn 4 dòng danh mục tham chiếu như liệt kê ở trên; thao tác đó chỉ được phép khi được phê duyệt cho database test mới, không chạy trong bước khảo sát này.

Trước khi chạy, cần xác nhận:

1. Tài khoản Windows có quyền tạo database và tạo schema trên đúng SQL Server instance.
2. `WareHouse_IntegrationTest` chưa tồn tại. Nếu đã tồn tại, dừng lại để chủ sở hữu xác minh; không ghi đè, không xóa, không migrate tự động.
3. Migration hiện tại là nguồn Code First duy nhất được dùng; không chạy `databaseA.sql`, seed script demo hoặc migration `Down`.
4. Target và instance được in/kiểm tra từ cùng tiến trình shell sẽ chạy migration.

## 4. Cấu hình connection string riêng

Không sửa `appsettings.json`, `appsettings.Development.json` hay connection string ứng dụng. Sau khi được PO duyệt, có thể truyền cấu hình riêng trong **một PowerShell session**:

```powershell
$env:WMS_TEST_CONNECTION_STRING = 'Server=localhost\SQL2022;Database=WareHouse_IntegrationTest;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True'
```

Integration test chỉ được đọc `WMS_TEST_CONNECTION_STRING`. Trước khi mở kết nối, fixture phải parse và từ chối chạy trừ khi:

- Data source đúng `localhost\SQL2022`.
- Initial catalog đúng `WareHouse_IntegrationTest` hoặc khớp prefix test tạm đã được PO duyệt.
- Dùng Windows Integrated Authentication; không có thông tin đăng nhập nhúng trong mã nguồn.
- Connection string không trỏ đến `WareHouse`, `A` hoặc `CommercialWMS`.

Nếu dùng cấu hình EF CLI bên dưới, chỉ set override trong PowerShell session đang thao tác; khôi phục giá trị cũ hoặc xóa biến ngay sau đó. Không chạy API khi biến override này đang bật.

## 5. Tạo database và áp dụng migration — CHỈ SAU KHI PO DUYỆT

Các lệnh sau là hướng dẫn tương lai, **không được chạy trong bước lập kế hoạch**. Trước tiên kiểm tra bằng truy vấn chỉ đọc trên `master` (không truy vấn `A`):

```powershell
sqlcmd -S 'localhost\SQL2022' -d master -E -b -Q "SELECT @@SERVERNAME AS ServerName, DB_ID(N'WareHouse_IntegrationTest') AS ExistingTestDatabaseId;"
```

Nếu database ID khác `NULL`, dừng lại và yêu cầu xác minh; không chạy migration. Nếu chưa tồn tại và PO đã duyệt, đặt biến cấu hình riêng ở mục 4 rồi chạy:

```powershell
if ([string]::IsNullOrWhiteSpace($env:WMS_TEST_CONNECTION_STRING)) {
    throw 'WMS_TEST_CONNECTION_STRING is required.'
}

$target = @{}
foreach ($part in $env:WMS_TEST_CONNECTION_STRING.Split(';')) {
    if ([string]::IsNullOrWhiteSpace($part)) {
        continue
    }

    $separator = $part.IndexOf('=')
    if ($separator -lt 1) {
        throw 'Refusing to run: malformed test connection string.'
    }

    $key = $part.Substring(0, $separator).Trim().ToLowerInvariant()
    $value = $part.Substring($separator + 1).Trim().Trim('"').Trim("'")
    $target[$key] = $value
}

if ($target['server'] -ne 'localhost\SQL2022' -or
    $target['database'] -ne 'WareHouse_IntegrationTest' -or
    $target['trusted_connection'] -ne 'True' -or
    $target.ContainsKey('user id') -or
    $target.ContainsKey('password')) {
    throw 'Refusing to run: test connection must use localhost\SQL2022, WareHouse_IntegrationTest, and Windows Integrated Authentication.'
}

$previousConnection = $env:ConnectionStrings__CommercialWms
try {
    $env:ConnectionStrings__CommercialWms = $env:WMS_TEST_CONNECTION_STRING
    dotnet ef database update `
      --project CommercialWms.Api\CommercialWms.Api.csproj `
      --startup-project CommercialWms.Api\CommercialWms.Api.csproj `
      --context CommercialWmsDbContext
    if ($LASTEXITCODE -ne 0) {
        throw 'EF Core migration failed; stop and inspect the test database.'
    }
}
finally {
    if ($null -eq $previousConnection) {
        Remove-Item Env:ConnectionStrings__CommercialWms -ErrorAction SilentlyContinue
    }
    else {
        $env:ConnectionStrings__CommercialWms = $previousConnection
    }
}
```

EF Core có thể tạo database đích nếu tài khoản và SQL Server cho phép. Đoạn trên kiểm tra target trước rồi ánh xạ connection string test vào `ConnectionStrings__CommercialWms` chỉ trong tiến trình hiện tại và khôi phục giá trị cũ. Parser này cố ý chỉ chấp nhận key `Server`, `Database`, `Trusted_Connection` và không cho phép user/password; dùng đúng định dạng mẫu ở mục 4. Integration-test fixture về sau nên dùng `Microsoft.Data.SqlClient.SqlConnectionStringBuilder` để xác thực connection string. Không dùng `--connection` nếu chưa xác nhận phiên bản/tooling và đảm bảo startup context không bị cấu hình connection string khác.

Sau migration, chỉ xác minh bằng SELECT trên chính database test:

```powershell
sqlcmd -S 'localhost\SQL2022' -d WareHouse_IntegrationTest -E -b -Q "SELECT DB_NAME() AS CurrentDatabase; SELECT MigrationId, ProductVersion FROM dbo.__EFMigrationsHistory; SELECT COUNT(*) AS UserTableCount FROM sys.tables WHERE is_ms_shipped = 0 AND name <> '__EFMigrationsHistory';"
```

Kết quả cần đối chiếu: database hiện tại là `WareHouse_IntegrationTest`, migration `20261116075251_InitialCreate` được ghi nhận, và có 56 bảng ứng dụng (không tính bảng lịch sử migration). Kiểm tra tiếp FK, CHECK, computed columns và 4 dòng danh mục tham chiếu; không coi dữ liệu workflow demo là fixture mặc định.

## 6. Fixture, chạy test và dọn dẹp

### Fixture an toàn

- Test InMemory hiện hữu tiếp tục chạy độc lập, không cần connection string hay SQL Server.
- Test SQL Server phải opt-in rõ ràng (ví dụ category/trait `SqlIntegration`) và bỏ qua hoặc báo lỗi cấu hình rõ nếu thiếu biến `WMS_TEST_CONNECTION_STRING`; không được âm thầm fallback sang appsettings.
- Tạo dữ liệu fixture tối thiểu, có mã định danh duy nhất theo `RunId`; không dùng bản ghi hiện có của `WareHouse`.
- Mỗi test phải kiểm tra kết quả ledger/balance và quan hệ liên quan. Test kiểm tra SQL constraints phải dùng SQL Server, không thay bằng InMemory.
- Tắt chạy song song nếu nhiều test dùng chung database cố định. Tốt hơn là database tạm riêng cho từng test run.
- Không gọi API ghi lên `WareHouse`, không chạy file seed demo, không dùng dữ liệu vận hành làm fixture.

### Lệnh chạy

Test service hiện tại (không kết nối SQL Server):

```powershell
dotnet test --project CommercialWms.Api.Tests\CommercialWms.Api.Tests.csproj
```

Sau khi đã bổ sung và được duyệt test SQL, đặt `WMS_TEST_CONNECTION_STRING` đúng database test rồi chạy lọc riêng test integration theo convention của project. Chạy riêng integration suite, kiểm tra log cho thấy server/database đích, rồi xóa biến khỏi session:

```powershell
Remove-Item Env:WMS_TEST_CONNECTION_STRING -ErrorAction SilentlyContinue
```

Lệnh lọc cụ thể cần được quyết định khi nhóm thêm trait/category vào test; hiện chưa có integration test hoặc trait như vậy.

### Dọn fixture

- Không dùng `EnsureDeleted`, `EnsureCreated`, `dotnet ef database drop`, `database update 0`, `DROP DATABASE`, hoặc lệnh cleanup tổng quát trỏ theo connection string hiện hành.
- Với database test cố định: cleanup chỉ xóa fixture thuộc `RunId` hiện tại theo thứ tự phụ thuộc FK trong **đúng database test**, trong `try/finally`; nếu cleanup thất bại, đánh dấu database cần kiểm tra thủ công, không fallback sang database khác.
- Với database tạm theo run: chỉ xóa database do harness tạo trong chính run đó, sau khi kiểm tra lại server và tên khớp chính xác prefix/RunId được allowlist. Việc xóa database là thao tác phá hủy, phải được PO phê duyệt riêng và không tự bật trong teardown CI cho đến khi cơ chế guard được review.
- Không bao giờ dọn bằng cách chạy migration `Down`; migration này drop toàn bộ 56 bảng.
- Ghi lại tên database, server, RunId, thời điểm, kết quả và fixture cleanup. Không ghi connection string đầy đủ hoặc bí mật vào log.

## 7. Rủi ro và biện pháp bảo vệ

| Rủi ro | Biện pháp |
|---|---|
| Test ghi nhầm `WareHouse` do appsettings local hoặc biến môi trường cũ | Test chỉ đọc `WMS_TEST_CONNECTION_STRING`; kiểm tra server/catalog bằng allowlist và fail-fast; không fallback. |
| Nhầm SQL instance hoặc target DB đã có dữ liệu | Preflight `DB_ID` trên `master`; nếu database tồn tại thì dừng; xác minh `DB_NAME()` trước/sau thao tác. |
| Migration vô tình ghi seed hoặc dữ liệu nghiệp vụ | Ghi nhận trước 4 dòng `HasData` trong migration; chỉ chạy trên DB test mới sau phê duyệt; không chạy seed demo. |
| Dọn test xóa nhầm dữ liệu/schema | Không dùng `EnsureDeleted`, `database drop` hoặc `Down`; cleanup giới hạn fixture/run. Xóa DB tạm cần phê duyệt riêng và guard exact name. |
| InMemory tạo cảm giác transaction SQL đã được kiểm thử | Giữ rõ nhãn unit vs SQL integration; các test rollback, FK, check, locking phải dùng SQL Server test target. |
| Test song song tranh chấp dữ liệu | Database riêng theo run hoặc tắt parallelization cho fixture DB dùng chung. |
| Connection string/credential bị lộ | Integrated Auth cho local; dùng biến môi trường/User Secrets; không đọc file local, ghi log connection string, hoặc commit secret. |
| Migration không chạy được từ database trống dù source đủ 56 bảng | Chỉ kết luận sau một lần tạo DB được duyệt; ghi lại lỗi và dừng, không sửa migration/schema trong P0.1. |

## 8. Các điểm cần PO phê duyệt

1. Cho phép tạo `WareHouse_IntegrationTest` trên `localhost\SQL2022` bằng migration hiện có; xác nhận database chưa tồn tại hoặc chỉ định xử lý nếu đã tồn tại.
2. Cho phép migration `InitialCreate` tạo schema và chèn bốn dòng reference seed; xác nhận rõ không seed dữ liệu nghiệp vụ.
3. Phê duyệt tài khoản/Windows identity có quyền tạo database/schema và chính sách giữ/xóa database test.
4. Chọn database cố định hay database tạm theo từng test run; phê duyệt riêng thao tác xóa database tạm nếu chọn phương án này.
5. Phê duyệt chính sách fixture cleanup, test song song, log/evidence và quyền truy cập của CI.
6. Phê duyệt phạm vi test HTTP/integration và package bổ sung nếu cần. Hiện tại chưa có SQL integration-test project/configuration.

## 9. Kết quả bước P0.1

- Đã khảo sát static: .NET/EF versions, cấu hình DI, migration, seed và test provider.
- Migration source có đủ 56 lệnh tạo bảng; khả năng dựng thành công trên SQL Server trống **chưa được xác nhận bằng thực thi**.
- Đã đề xuất target `WareHouse_IntegrationTest` và cơ chế cấu hình riêng, guard target, fixture và cleanup.
- Chưa tạo database, chưa chạy migration/SQL write, chưa thay đổi app connection string, code, schema, migration hoặc nghiệp vụ. Cần PO phê duyệt trước mọi bước tạo database hoặc ghi dữ liệu.
