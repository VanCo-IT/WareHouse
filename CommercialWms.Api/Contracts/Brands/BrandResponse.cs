namespace CommercialWms.Api.Contracts.Brands;

public sealed record BrandResponse(
    int BrandId,
    string BrandName,
    int CountryId,
    bool IsActive);
