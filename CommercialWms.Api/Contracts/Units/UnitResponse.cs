namespace CommercialWms.Api.Contracts.Units;

/// <summary>Unit of measure information returned by the API.</summary>
public sealed record UnitResponse(int UnitId, string UnitCode, string UnitName, bool IsActive);
