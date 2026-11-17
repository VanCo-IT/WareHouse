namespace CommercialWms.Api.Entities;

public sealed class Warehouse
{
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = null!;
    public string WarehouseName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}
