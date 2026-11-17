using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace CommercialWms.Api.Data;

public sealed class CommercialWmsDbContext(DbContextOptions<CommercialWmsDbContext> options) : DbContext(options)
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<CustomerReturnDetail> CustomerReturnDetails => Set<CustomerReturnDetail>();
    public DbSet<CustomerReturn> CustomerReturns => Set<CustomerReturn>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerType> CustomerTypes => Set<CustomerType>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<DeliveryDetail> DeliveryDetails => Set<DeliveryDetail>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<FoodSafetyDocument> FoodSafetyDocuments => Set<FoodSafetyDocument>();
    public DbSet<GoodsReceiptDetail> GoodsReceiptDetails => Set<GoodsReceiptDetail>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();
    public DbSet<InventoryLot> InventoryLots => Set<InventoryLot>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<ManufacturerDocument> ManufacturerDocuments => Set<ManufacturerDocument>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<PaymentDetail> PaymentDetails => Set<PaymentDetail>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductStorageRequirement> ProductStorageRequirements => Set<ProductStorageRequirement>();
    public DbSet<ProductUnit> ProductUnits => Set<ProductUnit>();
    public DbSet<ProductWarehousePolicy> ProductWarehousePolicies => Set<ProductWarehousePolicy>();
    public DbSet<PurchaseOrderDetail> PurchaseOrderDetails => Set<PurchaseOrderDetail>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchasePriceHistory> PurchasePriceHistories => Set<PurchasePriceHistory>();
    public DbSet<QualityInspectionDetail> QualityInspectionDetails => Set<QualityInspectionDetail>();
    public DbSet<QualityInspection> QualityInspections => Set<QualityInspection>();
    public DbSet<SalesOrderDetail> SalesOrderDetails => Set<SalesOrderDetail>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<StockAdjustmentDetail> StockAdjustmentDetails => Set<StockAdjustmentDetail>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<StockCountDetail> StockCountDetails => Set<StockCountDetail>();
    public DbSet<StockCount> StockCounts => Set<StockCount>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<StockTransferDetail> StockTransferDetails => Set<StockTransferDetail>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<SupplierContact> SupplierContacts => Set<SupplierContact>();
    public DbSet<SupplierDocument> SupplierDocuments => Set<SupplierDocument>();
    public DbSet<SupplierProduct> SupplierProducts => Set<SupplierProduct>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<WarehouseAisle> WarehouseAisles => Set<WarehouseAisle>();
    public DbSet<WarehouseBin> WarehouseBins => Set<WarehouseBin>();
    public DbSet<WarehouseEmployee> WarehouseEmployees => Set<WarehouseEmployee>();
    public DbSet<WarehouseRack> WarehouseRacks => Set<WarehouseRack>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseShelf> WarehouseShelves => Set<WarehouseShelf>();
    public DbSet<WarehouseTemperatureLog> WarehouseTemperatureLogs => Set<WarehouseTemperatureLog>();
    public DbSet<WarehouseZone> WarehouseZones => Set<WarehouseZone>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Remove(typeof(ForeignKeyIndexConvention));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommercialWmsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
