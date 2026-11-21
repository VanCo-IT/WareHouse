namespace CommercialWms.Api.Contracts.Products;

public sealed record ProductResponse(
    int ProductId,
    string SKU,
    string ProductName,
    int CategoryId,
    int? UnitId,
    int? BrandId,
    string? Barcode,
    string? Description,
    bool IsPerishable,
    bool IsActive,
    DateTime CreatedAt);
