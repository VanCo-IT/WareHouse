using CommercialWms.Api.Contracts.WarehouseBins;

namespace CommercialWms.Api.Contracts.Warehouses;

/// <summary>Warehouse and its complete schema-backed storage hierarchy.</summary>
public sealed record WarehouseStructureResponse(
    WarehouseResponse Warehouse,
    IReadOnlyList<WarehouseZoneNodeResponse> Zones);

/// <summary>Zone node containing its aisles.</summary>
public sealed record WarehouseZoneNodeResponse(
    int ZoneId,
    string ZoneCode,
    string ZoneName,
    IReadOnlyList<WarehouseAisleNodeResponse> Aisles);

/// <summary>Aisle node containing its racks.</summary>
public sealed record WarehouseAisleNodeResponse(
    int AisleId,
    string AisleCode,
    IReadOnlyList<WarehouseRackNodeResponse> Racks);

/// <summary>Rack node containing its shelves.</summary>
public sealed record WarehouseRackNodeResponse(
    int RackId,
    string RackCode,
    IReadOnlyList<WarehouseShelfNodeResponse> Shelves);

/// <summary>Shelf node containing its bins.</summary>
public sealed record WarehouseShelfNodeResponse(
    int ShelfId,
    string ShelfCode,
    IReadOnlyList<WarehouseBinResponse> Bins);
