namespace CommercialWms.Api.Contracts.Suppliers;

public sealed record SupplierResponse(
    int SupplierId,
    string SupplierCode,
    string SupplierName,
    int CountryId,
    string? TaxCode,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive);
