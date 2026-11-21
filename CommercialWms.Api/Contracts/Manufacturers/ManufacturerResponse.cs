namespace CommercialWms.Api.Contracts.Manufacturers;

/// <summary>Manufacturer information returned by the API.</summary>
public sealed record ManufacturerResponse(
    int ManufacturerId,
    string ManufacturerCode,
    string ManufacturerName,
    int CountryId,
    string? Address,
    bool IsActive);
