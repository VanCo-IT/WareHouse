namespace CommercialWms.Api.Entities;

public sealed class StockCount
{
    public long StockCountId { get; set; }
    public string StockCountNumber { get; set; } = null!;
    public int WarehouseId { get; set; }
    public DateTime CountDate { get; set; }
    public string Status { get; set; } = null!;
    public int? CreatedByEmployeeId { get; set; }
}
