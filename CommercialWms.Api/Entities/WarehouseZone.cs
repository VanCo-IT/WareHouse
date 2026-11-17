namespace CommercialWms.Api.Entities;

public sealed class WarehouseZone
{
    public int ZoneId { get; set; }
    public int WarehouseId { get; set; }
    public string ZoneCode { get; set; } = null!;
    public string ZoneName { get; set; } = null!;
}
