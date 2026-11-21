namespace CommercialWms.Api.Contracts.WarehouseAisles;

/// <summary>Warehouse aisle data returned by the API.</summary>
public sealed record WarehouseAisleResponse(int AisleId, int ZoneId, string AisleCode);
