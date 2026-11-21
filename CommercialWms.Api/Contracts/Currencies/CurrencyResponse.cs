namespace CommercialWms.Api.Contracts.Currencies;

/// <summary>Currency information returned by the API.</summary>
public sealed record CurrencyResponse(int CurrencyId, string CurrencyCode, string CurrencyName, bool IsActive);
