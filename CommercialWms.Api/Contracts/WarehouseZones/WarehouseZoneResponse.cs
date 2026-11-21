namespace CommercialWms.Api.Contracts.WarehouseZones;

/// <summary>Warehouse zone data returned by the API.</summary>
public sealed record WarehouseZoneResponse(
    int ZoneId,
    int WarehouseId,
    string ZoneCode,
    string ZoneName);
