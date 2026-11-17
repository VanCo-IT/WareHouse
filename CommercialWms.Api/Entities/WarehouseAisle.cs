namespace CommercialWms.Api.Entities;

public sealed class WarehouseAisle
{
    public int AisleId { get; set; }
    public int ZoneId { get; set; }
    public string AisleCode { get; set; } = null!;
}
