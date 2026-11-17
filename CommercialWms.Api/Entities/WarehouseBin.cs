namespace CommercialWms.Api.Entities;

public sealed class WarehouseBin
{
    public int BinId { get; set; }
    public int ShelfId { get; set; }
    public string BinCode { get; set; } = null!;
    public decimal? Capacity { get; set; }
    public bool IsActive { get; set; } = true;
}
