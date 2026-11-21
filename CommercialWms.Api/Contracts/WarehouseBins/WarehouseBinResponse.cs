namespace CommercialWms.Api.Contracts.WarehouseBins;

/// <summary>Warehouse bin data returned by the API.</summary>
public sealed record WarehouseBinResponse(
    int BinId,
    int ShelfId,
    string BinCode,
    decimal? Capacity,
    bool IsActive);
