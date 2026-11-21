namespace CommercialWms.Api.Contracts.WarehouseShelves;

/// <summary>Warehouse shelf data returned by the API.</summary>
public sealed record WarehouseShelfResponse(int ShelfId, int RackId, string ShelfCode);
