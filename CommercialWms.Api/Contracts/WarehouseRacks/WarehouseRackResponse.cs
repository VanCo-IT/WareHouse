namespace CommercialWms.Api.Contracts.WarehouseRacks;

/// <summary>Warehouse rack data returned by the API.</summary>
public sealed record WarehouseRackResponse(int RackId, int AisleId, string RackCode);
