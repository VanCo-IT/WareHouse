namespace CommercialWms.Api.Contracts.Suppliers;

public sealed record SupplierProductResponse(
    long SupplierProductId,
    int SupplierId,
    int ProductId,
    string ProductSKU,
    string ProductName,
    int ManufacturerId,
    string ManufacturerName,
    string? SupplierProductCode,
    int LeadTimeDays,
    bool IsActive);
