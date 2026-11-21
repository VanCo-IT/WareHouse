# Y2 — User Stories, Product/Release/Sprint Backlog

## 1. Nguồn và trạng thái

Backlog này được xây dựng từ [WMS implementation traceability](../../WMS%20Business%20Documentation/06-implementation-traceability.md), [API and data map](../../WMS%20Business%20Documentation/07-api-and-data-map.md), [Y1 Functional Decomposition](../Y1/Y1-Functional-Decomposition.md), [Y1 Work Breakdown](../Y1/Y1-Work-Breakdown.md) và [Y1 Team Assignment](../Y1/Y1-Team-Assignment.md).

- `Có API + UI`: có endpoint và giao diện tương ứng trong source; không đồng nghĩa đã kiểm thử hoặc hoàn tất trong một Sprint.
- `Chỉ schema/database`: có mô hình dữ liệu nhưng chưa có API/UI nghiệp vụ.
- `Đề xuất`: scope/AC tương lai cần PO xác nhận.
- Các ưu tiên và Business Value trong bảng là **đề xuất ban đầu**, chưa được PO/nhóm duyệt. Rank `1` là mức ưu tiên cao nhất trong danh sách hiện tại.
- Tất cả State, ngoại trừ nội dung mô tả hiện trạng, để `[CẦN XÁC NHẬN]`; không ghi nhận trạng thái hoàn thành.
- Sheet `RULE` mô tả Business Value theo thang 1–100, còn các dòng minh họa trong backlog dùng High/Medium. Workbook dùng High/Medium theo định dạng ví dụ; cần PO/giảng viên xác nhận có phải đổi sang thang số hay không.

## 2. Product Backlog — User Stories

### US-01 — Quản lý danh mục sản phẩm

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to xem, tạo, cập nhật và xóa danh mục sản phẩm.
- **Giá trị:** So that sản phẩm có thể được tổ chức theo danh mục.
- **Acceptance Criteria:**
  1. GET danh sách/chi tiết trả Category DTO; ID không tồn tại trả 404.
  2. Tạo/cập nhật yêu cầu CategoryCode 1–30 ký tự và CategoryName 1–100 ký tự.
  3. Parent nếu được cung cấp phải tồn tại; parent tự tham chiếu bị từ chối.
  4. CategoryCode trùng hoặc xóa danh mục còn Product/danh mục con bị từ chối theo API.
- **Priority / Business Value:** 1 / High — đề xuất, chờ PO xác nhận.
- **Y1:** 1.1 Quản lý danh mục sản phẩm.
- **Hiện trạng:** Có API + UI; thao tác ghi chưa được kiểm thử trong đợt lập tài liệu.

### US-02 — Quản lý thương hiệu

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to tra cứu và quản lý thương hiệu.
- **Giá trị:** So that sản phẩm có thể được gắn với thương hiệu phù hợp.
- **Acceptance Criteria:**
  1. GET danh sách/chi tiết trả Brand DTO; ID không tồn tại trả 404.
  2. Country được tham chiếu phải tồn tại.
  3. Cặp BrandName/Country trùng bị từ chối.
  4. Không xóa Brand còn Product tham chiếu; UI hiển thị lỗi API thay vì báo thành công.
- **Priority / Business Value:** 2 / High — đề xuất, chờ PO xác nhận.
- **Y1:** 1.2 Quản lý thương hiệu.
- **Hiện trạng:** Có API + UI; thao tác ghi chưa được kiểm thử trong đợt lập tài liệu.

### US-03 — Quản lý sản phẩm

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to tra cứu, tạo, cập nhật và xóa sản phẩm.
- **Giá trị:** So that thông tin catalog sản phẩm được duy trì tập trung.
- **Acceptance Criteria:**
  1. GET danh sách/chi tiết trả Product DTO; ID không tồn tại trả 404.
  2. SKU và ProductName bắt buộc theo giới hạn DTO; SKU/barcode trùng bị từ chối.
  3. Category phải tồn tại; Brand nếu có phải tồn tại và đang active.
  4. Không xóa Product đang được dữ liệu nghiệp vụ tham chiếu.
- **Priority / Business Value:** 3 / High — đề xuất, chờ PO xác nhận.
- **Y1:** 1.3 Quản lý sản phẩm.
- **Hiện trạng:** Có API + UI; thao tác ghi chưa được kiểm thử trong đợt lập tài liệu.

### US-04 — Quản lý nhà cung cấp

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to tra cứu, tạo, cập nhật và đổi trạng thái nhà cung cấp.
- **Giá trị:** So that thông tin nguồn cung được duy trì nhất quán.
- **Acceptance Criteria:**
  1. GET danh sách/chi tiết trả Supplier DTO; ID không tồn tại trả 404.
  2. SupplierCode duy nhất; Country được tham chiếu phải tồn tại.
  3. Hỗ trợ tạo/cập nhật/đổi trạng thái theo API hiện có.
  4. Không hiển thị thao tác xóa nhà cung cấp vì API hiện không có DELETE.
- **Priority / Business Value:** 4 / High — đề xuất, chờ PO xác nhận.
- **Y1:** 2.1 Quản lý nhà cung cấp.
- **Hiện trạng:** Có API + UI; ghi chưa được kiểm thử trong đợt lập tài liệu.

### US-05 — Quản lý liên hệ nhà cung cấp

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to quản lý các đầu mối liên hệ thuộc một nhà cung cấp.
- **Giá trị:** So that người dùng có thể tra cứu thông tin liên lạc nhà cung cấp.
- **Acceptance Criteria:**
  1. GET/POST/PUT/DELETE sử dụng route contacts theo Supplier hiện có.
  2. Contact được thao tác trong đúng Supplier; Supplier/Contact không tồn tại trả 404 theo API.
  3. Dữ liệu nhập tuân thủ giới hạn DTO và lỗi được thể hiện rõ trên UI.
- **Priority / Business Value:** 5 / Medium — đề xuất, chờ PO xác nhận.
- **Y1:** 2.2 Quản lý liên hệ nhà cung cấp.
- **Hiện trạng:** Có API + UI trong trang chi tiết Supplier; ghi chưa được kiểm thử.

### US-06 — Liên kết nhà cung cấp, sản phẩm và nhà sản xuất

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to quản lý liên kết Supplier–Product–Manufacturer.
- **Giá trị:** So that nguồn cung của sản phẩm có thể được tra cứu.
- **Acceptance Criteria:**
  1. GET/POST/PUT/DELETE sử dụng endpoint Supplier Products hiện có.
  2. Các tham chiếu phải hợp lệ và bộ ba Supplier/Product/Manufacturer không trùng.
  3. Không xóa liên kết có lịch sử mua hàng.
  4. Cách tra cứu/chọn Manufacturer phải được xác nhận vì hiện chưa có API lookup riêng.
- **Priority / Business Value:** 6 / Medium — đề xuất, chờ PO xác nhận.
- **Y1:** 2.3 Liên kết Supplier–Product–Manufacturer.
- **Hiện trạng:** Có API + UI; cần xác nhận usability của lookup Manufacturer.

### US-07 — Quản lý kho và vị trí

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to quản lý kho và cấu trúc vị trí lưu trữ.
- **Giá trị:** So that hàng hóa có thể được định vị theo cấu trúc kho đã thống nhất.
- **Acceptance Criteria dự kiến — PO xác nhận:**
  1. PO xác nhận trường dữ liệu, thao tác được phép, actor và vòng đời của Warehouse/Zone/Aisle/Rack/Shelf/Bin.
  2. API/UI chỉ cho lưu quan hệ vị trí hợp lệ theo FK; dữ liệu không hợp lệ trả lỗi rõ ràng.
  3. Test evidence bao phủ đọc cấu trúc cha–con và các lỗi tham chiếu.
- **Priority / Business Value:** 7 / High — đề xuất, chờ PO xác nhận.
- **Y1:** 3.1–3.5.
- **Hiện trạng:** Chỉ schema/database; chưa có API/UI nghiệp vụ.

### US-08 — Mua hàng và nhập kho

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to ghi nhận đơn mua và hàng nhận theo từng dòng.
- **Giá trị:** So that số lượng hàng đặt và nhận có thể được theo dõi trong hệ thống.
- **Acceptance Criteria dự kiến — PO xác nhận:**
  1. PO xác nhận trường bắt buộc, luồng trạng thái, chính sách sửa/hủy và liên kết chứng từ.
  2. Purchase Order/Details và Goods Receipt/Details được liên kết theo schema; tham chiếu không hợp lệ bị từ chối.
  3. Goods Receipt Details phải thỏa CHECK hiện có: AcceptedQuantity + RejectedQuantity không vượt ReceivedQuantity.
  4. Quy tắc giá mua, tiền tệ và xử lý chênh lệch cần được PO xác nhận trước triển khai.
- **Priority / Business Value:** 8 / High — đề xuất, chờ PO xác nhận.
- **Y1:** 4.1–4.4.
- **Hiện trạng:** Chỉ schema/database; chưa có API/UI nghiệp vụ.

### US-09 — Kiểm tra chất lượng

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to ghi nhận kết quả kiểm tra chất lượng hàng nhận.
- **Giá trị:** So that kết quả kiểm định được liên kết với lô hàng/nhận hàng theo quy trình được duyệt.
- **Acceptance Criteria dự kiến — PO xác nhận:**
  1. PO xác nhận phạm vi kiểm tra, tiêu chí đạt/không đạt, actor và hành động tiếp theo.
  2. Inspection và detail chỉ tham chiếu các hồ sơ liên quan hợp lệ theo schema.
  3. Kết quả và lỗi kiểm tra được lưu/hiển thị bằng API/UI sau khi workflow được phê duyệt.
- **Priority / Business Value:** 9 / High — đề xuất, chờ PO xác nhận.
- **Y1:** 5.1–5.2.
- **Hiện trạng:** Chỉ schema/database; chưa có API/UI nghiệp vụ.

### US-10 — Tồn kho và lô hàng

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to tra cứu tồn kho theo sản phẩm, vị trí và lô.
- **Giá trị:** So that số lượng và thông tin lô/hạn dùng có thể được theo dõi.
- **Acceptance Criteria dự kiến — PO xác nhận:**
  1. PO xác nhận truy vấn, trường hiển thị, quyền và chính sách FIFO/FEFO hoặc chính sách khác.
  2. Lot/Balance/Transaction liên kết theo tham chiếu hợp lệ; không vượt CHECK `QuantityReserved <= QuantityOnHand`.
  3. Quy tắc tạo/gỡ reservation và cách duy trì số dư phải được định nghĩa trước triển khai.
- **Priority / Business Value:** 10 / High — đề xuất, chờ PO xác nhận.
- **Y1:** 6.1–6.5.
- **Hiện trạng:** Chỉ schema/database; chưa có API/UI nghiệp vụ.

### US-11 — Điều chuyển và điều chỉnh tồn kho

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to ghi nhận điều chuyển và điều chỉnh tồn kho có lý do.
- **Giá trị:** So that thay đổi vị trí/số lượng được lưu vết.
- **Acceptance Criteria dự kiến — PO xác nhận:**
  1. PO xác nhận lý do, phê duyệt, trạng thái và quyền được phép thực hiện.
  2. Transfer phải có kho nguồn khác kho đích; ReceivedQuantity không vượt RequestedQuantity.
  3. DifferenceQuantity của StockAdjustmentDetails được tính bởi database, không nhập như giá trị người dùng.
  4. Các cập nhật tồn kho và audit trail chỉ được xác định sau khi luồng nghiệp vụ được duyệt.
- **Priority / Business Value:** 11 / High — đề xuất, chờ PO xác nhận.
- **Y1:** 7.1–7.3.
- **Hiện trạng:** Chỉ schema/database; chưa có API/UI nghiệp vụ.

### US-12 — Khách hàng và địa chỉ

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to quản lý thông tin khách hàng và địa chỉ giao nhận.
- **Giá trị:** So that thông tin khách hàng có thể được dùng trong nghiệp vụ bán hàng/giao hàng.
- **Acceptance Criteria dự kiến — PO xác nhận:**
  1. PO xác nhận trường bắt buộc, quy tắc trùng lặp, quyền xem/sửa và chính sách dữ liệu cá nhân.
  2. Customer và Address liên kết hợp lệ theo schema; lỗi tham chiếu được báo rõ.
  3. API/UI chỉ được coi là hoàn thành sau khi được kiểm thử theo tiêu chí đã duyệt.
- **Priority / Business Value:** 12 / High — đề xuất, chờ PO xác nhận.
- **Y1:** 9.1–9.3.
- **Hiện trạng:** Chỉ schema/database; chưa có API/UI nghiệp vụ.

### US-13 — Đơn bán hàng

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to lập và theo dõi đơn bán hàng cùng các dòng sản phẩm.
- **Giá trị:** So that nhu cầu bán hàng được lưu theo khách hàng và sản phẩm.
- **Acceptance Criteria dự kiến — PO xác nhận:**
  1. PO xác nhận trạng thái, trường giá/chiết khấu/thuế, điều kiện sửa/hủy và cách giữ hàng.
  2. Sales Order/Details chỉ lưu với các tham chiếu hợp lệ theo schema.
  3. Quy tắc trừ/giữ tồn và xử lý thiếu hàng chưa được suy diễn từ schema; phải được phê duyệt riêng.
- **Priority / Business Value:** 13 / High — đề xuất, chờ PO xác nhận.
- **Y1:** 8.1.
- **Hiện trạng:** Chỉ schema/database; chưa có API/UI nghiệp vụ.

### US-14 — Xuất kho và giao hàng

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to theo dõi việc chuẩn bị và giao hàng theo đơn bán.
- **Giá trị:** So that trạng thái giao hàng và các dòng giao có thể được tra cứu.
- **Acceptance Criteria dự kiến — PO xác nhận:**
  1. PO xác nhận picking/allocation, giao một phần, giao thất bại và trạng thái giao hàng.
  2. Delivery/Details liên kết với hồ sơ bán hàng và vị trí/lô theo quy tắc được duyệt.
  3. Số lượng giao và cập nhật tồn kho được kiểm tra bằng test đã duyệt; không tự giả định chính sách xuất.
- **Priority / Business Value:** 14 / High — đề xuất, chờ PO xác nhận.
- **Y1:** 8.2 và 8.5.
- **Hiện trạng:** Chỉ schema/database; chưa có API/UI nghiệp vụ.

### US-15 — Trả hàng khách hàng

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to ghi nhận hàng khách trả và kết quả xử lý.
- **Giá trị:** So that hàng trả được theo dõi theo hồ sơ bán/giao liên quan.
- **Acceptance Criteria dự kiến — PO xác nhận:**
  1. PO xác nhận điều kiện nhận trả, trạng thái hàng, kiểm tra chất lượng và quyết định nhập lại/loại bỏ.
  2. Return/Details liên kết đúng hồ sơ gốc theo schema.
  3. Tác động tồn kho và hoàn tiền không được suy diễn; chỉ triển khai theo quy tắc được duyệt.
- **Priority / Business Value:** 15 / Medium — đề xuất, chờ PO xác nhận.
- **Y1:** 8.3 và 8.5.
- **Hiện trạng:** Chỉ schema/database; chưa có API/UI nghiệp vụ.

### US-16 — Thanh toán

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to ghi nhận thanh toán và phân bổ vào hồ sơ bán hàng.
- **Giá trị:** So that tình trạng thanh toán có thể được đối soát theo chính sách đã duyệt.
- **Acceptance Criteria dự kiến — PO xác nhận:**
  1. PO xác nhận phương thức, tiền tệ, thanh toán một phần, hoàn tiền, phí và cách xử lý sai lệch.
  2. Payment/Details liên kết hợp lệ theo schema.
  3. Quy tắc tổng tiền, làm tròn, trạng thái và đối soát phải được duyệt trước khi xây API/UI.
- **Priority / Business Value:** 16 / Medium — đề xuất, chờ PO xác nhận.
- **Y1:** 8.4.
- **Hiện trạng:** Chỉ schema/database; chưa có API/UI nghiệp vụ.

### US-17 — Báo cáo vận hành

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to xem báo cáo tồn kho và hoạt động theo bộ lọc được duyệt.
- **Giá trị:** So that người dùng có thể theo dõi tình hình vận hành dựa trên dữ liệu hệ thống.
- **Acceptance Criteria dự kiến — PO xác nhận:**
  1. PO xác nhận chỉ số, công thức, bộ lọc, phạm vi thời gian và quyền truy cập từng báo cáo.
  2. Kết quả được đối chiếu với truy vấn nguồn/định nghĩa nghiệp vụ đã duyệt.
  3. Không xem Dashboard đếm catalog hiện có là báo cáo vận hành.
- **Priority / Business Value:** 17 / Medium — đề xuất, chờ PO xác nhận.
- **Y1:** 10.4–10.5; dashboard catalog hiện có nằm ở 10.1.
- **Hiện trạng:** Báo cáo vận hành chưa triển khai API/UI; dashboard hiện tại chỉ có số liệu catalog.

### US-18 — Kiểm kê kho

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to lập và ghi nhận kết quả kiểm kê theo kho/vị trí.
- **Giá trị:** So that chênh lệch giữa số hệ thống và số thực đếm có thể được xem xét.
- **Acceptance Criteria dự kiến — PO xác nhận:**
  1. PO xác nhận phương thức kiểm kê, phạm vi khóa giao dịch, số lần đếm và phê duyệt.
  2. StockCount/Details lưu dữ liệu theo schema; DifferenceQuantity được tính bởi database, không nhập tay.
  3. Cách áp dụng chênh lệch vào tồn kho cần được duyệt và kiểm thử riêng.
- **Priority / Business Value:** 18 / Medium — đề xuất, chờ PO xác nhận.
- **Y1:** 10.2.
- **Hiện trạng:** Chỉ schema/database; chưa có API/UI nghiệp vụ.

### US-19 — Cấu hình, audit và nhiệt độ kho

- **Vai trò:** As a/an `[CẦN XÁC NHẬN]`
- **Mong muốn:** I want to quản lý cấu hình và tra cứu các nhật ký hệ thống/kho được cho phép.
- **Giá trị:** So that các thay đổi và điều kiện vận hành cần theo dõi có thể được xem xét.
- **Acceptance Criteria dự kiến — PO xác nhận:**
  1. PO xác nhận cấu hình được phép thay đổi, quyền xem, lưu giữ audit và ngưỡng/cách xử lý nhiệt độ.
  2. AuditLogs/SystemSettings/WarehouseTemperatureLogs được truy cập qua API/UI với quyền đã duyệt.
  3. Việc ghi log/đo nhiệt độ tự động hay thủ công, thời gian lưu và cảnh báo đều cần quyết định trước khi phát triển.
- **Priority / Business Value:** 19 / Medium — đề xuất, chờ PO xác nhận.
- **Y1:** 10.3 và 3.4.
- **Hiện trạng:** Chỉ schema/database; chưa có API/UI nghiệp vụ.

## 3. Release Backlog đề xuất

Release grouping là thứ tự chức năng tham đề xuất, **không có ngày phát hành**. `REL-01`–`REL-04` là ID nhóm Release; cột Story ID trên từng dòng liên kết tới một `US-*`. Sprint Backlog liên kết tới Release bằng `REL-*` và tới story bằng `US-*`.

| Release | Mục tiêu đề xuất | User Stories | Sprint |
|---|---|---|---|
| REL-01 — Catalog và nhà cung cấp | Hoàn thiện/kiểm chứng catalog và thông tin nguồn cung đang có API/UI | US-01–US-06 | Chỉ US-01–US-03 được chọn vào Sprint 1 đề xuất; phần còn lại `[CẦN XÁC NHẬN]` |
| REL-02 — Kho, nhập hàng và kiểm định | Xác nhận nghiệp vụ nền, sau đó xây workflow kho/nhập/kiểm định/tồn | US-07–US-10 | `[CẦN XÁC NHẬN]` |
| REL-03 — Dịch chuyển và bán hàng | Điều chuyển/điều chỉnh, khách hàng, đơn bán, giao hàng | US-11–US-14 | `[CẦN XÁC NHẬN]` |
| REL-04 — Hậu mãi và quản trị | Trả hàng, thanh toán, báo cáo, kiểm kê, cấu hình/log | US-15–US-19 | `[CẦN XÁC NHẬN]` |

## 4. Sprint Backlog đề xuất

### Sprint Goal

**Đề xuất:** “Kiểm chứng trong môi trường cô lập các luồng hiện có của Categories, Brands và Products từ giao diện tới API; ghi nhận kết quả và các vấn đề cần PO quyết định.”

Đây là Sprint planning candidate, không phải Sprint đã diễn ra hoặc đã được nhóm cam kết. Sprint length, start/end date, capacity, estimate, State và DoD đều `[CẦN XÁC NHẬN]`. Không ghi dữ liệu thử vào `WareHouse` dùng chung.

### User Story được chọn

| Story | Lý do chọn | Điều kiện trước khi nhận Sprint |
|---|---|---|
| US-01 Categories | API + UI đang có; có tiêu chí mã/parent/không xóa khi đang được dùng | PO duyệt role/AC và có test environment cô lập |
| US-02 Brands | API + UI đang có; có ràng buộc Country và uniqueness | PO duyệt role/AC và có test environment cô lập |
| US-03 Products | API + UI đang có; có SKU/barcode/reference checks | PO duyệt role/AC và có test environment cô lập |

### Task cụ thể

| Task ID | Story / Release ID | Người phụ trách | Task và tiêu chí chấp nhận |
|---|---|---|---|
| S1-T01 | US-01–US-03 / REL-01 | Văn Có — Product Owner | Xác nhận vai trò, Sprint Goal, AC, thứ tự ưu tiên và môi trường được phép; quyết định được ghi lại. Nếu chưa xác nhận, scope vẫn là đề xuất. |
| S1-T02 | US-01–US-03 / REL-01 | Thế Anh — Scrum Master | Chuẩn bị checklist planning/review và ghi các phụ thuộc, quyết định, impediment nếu thực sự phát sinh; không ghi meeting đã diễn ra khi chưa có bằng chứng. |
| S1-T03 | US-01–US-03 / REL-01 | Hoàng — Developer + Tester | Thực hiện kịch bản đọc/ghi đã duyệt trên môi trường cô lập; actual result và evidence được lưu cho từng AC; defect nếu có phải có bằng chứng. Không thao tác ghi lên database dùng chung. |
| S1-T04 | US-01–US-03 / REL-01 | Nam — Database Developer | Rà soát schema/constraint và điều kiện dữ liệu cần cho test cô lập; cung cấp checklist tham chiếu; không sửa schema/migration hoặc ghi vào `WareHouse`. |
| S1-T05 | US-01–US-03 / REL-01 | Giang — Viết báo cáo | Duy trì liên kết story–AC–test evidence và ghi rõ mục chưa kiểm chứng; chỉ cập nhật kết quả sau khi nhận evidence thực tế. |
| S1-T06 | US-01–US-03 / REL-01 | Khải — Thuyết trình | Chuẩn bị dàn ý demo Catalog từ scope đã được duyệt và kết quả đã kiểm chứng; không trình bày chức năng chưa có bằng chứng như đã hoàn thành. |

Estimate (giờ/story points), ngày, State và reviewer cho từng task: `[CẦN XÁC NHẬN]`.

## 5. Các quyết định PO/nhóm còn thiếu

- Vai trò người dùng và quyền cho từng Use Case.
- Product Goal, Release Goal, thứ tự backlog và mức Business Value.
- Xác nhận AC cho các story tương lai; đặc biệt trạng thái/phê duyệt, tồn kho, FEFO/FIFO, giá/thuế, picking, returns/refunds, reports, audit retention và temperature thresholds.
- Có chấp nhận Sprint 1 candidate và môi trường kiểm thử cô lập hay không.
- Sprint number, thời lượng, lịch, capacity, estimate, State vocabulary, DoR/DoD và reviewer.
- Quy trình ghi actual results/defects, Trello URL và người cập nhật sau khi hoạt động thực sự diễn ra.

