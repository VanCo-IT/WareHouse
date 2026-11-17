namespace CommercialWms.Api.Entities;

public sealed class WarehouseRack
{
    public int RackId { get; set; }
    public int AisleId { get; set; }
    public string RackCode { get; set; } = null!;
}
