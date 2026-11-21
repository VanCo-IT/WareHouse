namespace CommercialWms.Api.Contracts.Warehouses;

/// <summary>Warehouse data returned by the API.</summary>
public sealed record WarehouseResponse(
    int WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    string Address,
    bool IsActive);
