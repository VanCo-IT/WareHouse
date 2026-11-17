namespace CommercialWms.Api.Contracts.Suppliers;

public sealed record SupplierContactResponse(
    int ContactId,
    int SupplierId,
    string ContactName,
    string? JobTitle,
    string? Email,
    string? Phone);
