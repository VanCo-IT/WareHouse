# 6. Bảng đối chiếu chức năng và trạng thái triển khai

**API + UI** nghĩa là đã thấy route và trang tương ứng trong source. Không có nghĩa mọi thao tác ghi đã được kiểm thử. **Schema/database only** không phải chức năng đã hoàn thành.

| Nhóm | Thành phần | API | UI | Database/entity | Trạng thái |
|---|---|---:|---:|---:|---|
| Product Catalog | Categories | Có | Có | Có | API + UI |
| Product Catalog | Brands | Có | Có | Có | API + UI |
| Product Catalog | Products | Có | Có | Có | API + UI |
| Product Catalog | ProductUnits, Units, StorageRequirements, WarehousePolicies | Không có API riêng | Không | Có | Schema/database only |
| Suppliers and Manufacturers | Suppliers | Có | Có | Có | API + UI |
| Suppliers and Manufacturers | SupplierContacts | Có | Có trong trang chi tiết | Có | API + UI |
| Suppliers and Manufacturers | SupplierProducts | Có | Có trong trang chi tiết | Có | API + UI |
| Suppliers and Manufacturers | Manufacturers, documents, food safety documents | Không | Không | Có | Schema/database only |
| Warehouses and Locations | Warehouses, Zones, Aisles, Racks, Shelves, Bins, WarehouseEmployees | Không | Không; menu báo đang phát triển | Có | Schema/database only |
| Customers and Employees | Customers, Addresses, Types, Employees | Không | Không | Có | Schema/database only |
| Purchasing, Receiving and Quality | PurchaseOrders, GoodsReceipts, QualityInspections và details | Không | Không; menu nhập hàng chưa có API | Có | Schema/database only |
| Inventory, Lots, Transfers and Adjustments | Lots, Balances, Transactions, Transfers, Adjustments, Reservations | Không | Không; menu tồn/xuất đang phát triển | Có | Schema/database only |
| Sales, Deliveries, Returns and Payments | SalesOrders, Deliveries, Returns, Payments | Không | Không | Có | Schema/database only |
| Stock Count and System | StockCounts, SystemSettings, TemperatureLogs, AuditLogs | Không | Không | Có | Schema/database only |

## Màn hình theo menu hiện tại

| Màn hình | Hiện trạng |
|---|---|
| Tổng quan | Có dashboard dựa trên số liệu catalog |
| Danh mục, Thương hiệu, Sản phẩm, Nhà cung cấp | Có UI và API |
| Chi tiết nhà cung cấp | Có UI/API cho contacts và supplier products |
| Kho hàng, Nhập hàng, Tồn kho, Xuất hàng, Báo cáo, Cài đặt | Hiển thị “Đang phát triển”, không dùng dữ liệu giả |
| Khách hàng, thanh toán, kiểm kê, trả hàng | Chưa thấy màn hình/API riêng |

## Tài nguyên database

56 bảng nghiệp vụ gồm:

- Catalog: ProductCategories, Products, Brands, ProductStorageRequirements, ProductUnits, ProductWarehousePolicies, Units.
- Nhà cung cấp/nhà sản xuất: Suppliers, SupplierContacts, SupplierProducts, SupplierDocuments, Manufacturers, ManufacturerDocuments, FoodSafetyDocuments, Countries.
- Kho/vị trí: Warehouses, WarehouseZones, WarehouseAisles, WarehouseRacks, WarehouseShelves, WarehouseBins, WarehouseEmployees, WarehouseTemperatureLogs.
- Khách hàng/nhân viên: Customers, CustomerAddresses, CustomerTypes, Employees.
- Mua hàng/nhận hàng/chất lượng: PurchaseOrders, PurchaseOrderDetails, PurchasePriceHistories, GoodsReceipts, GoodsReceiptDetails, QualityInspections, QualityInspectionDetails, Currencies, ExchangeRates.
- Tồn kho/lô/điều chuyển/điều chỉnh: InventoryLots, InventoryBalances, InventoryTransactions, StockTransfers, StockTransferDetails, StockAdjustments, StockAdjustmentDetails, StockReservations.
- Bán hàng/giao hàng/trả hàng/thanh toán: SalesOrders, SalesOrderDetails, Deliveries, DeliveryDetails, CustomerReturns, CustomerReturnDetails, Payments, PaymentDetails.
- Kiểm kê/hệ thống: StockCounts, StockCountDetails, AuditLogs, SystemSettings.

Sơ đồ quan hệ mức domain tại [erd-overview.mmd](diagrams/erd-overview.mmd) chỉ vẽ các quan hệ tiêu biểu, không thay thế toàn bộ 101 FK.
