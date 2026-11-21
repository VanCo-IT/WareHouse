namespace CommercialWms.Api.Contracts.Countries;

/// <summary>Country information returned by the API.</summary>
public sealed record CountryResponse(int CountryId, string CountryCode, string CountryName, bool IsActive);
