namespace CommercialWms.Api.Entities;

public sealed class ProductWarehousePolicy
{
    public long PolicyId { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public decimal MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public decimal SafetyQuantity { get; set; }
}
