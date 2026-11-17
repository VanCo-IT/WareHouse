namespace CommercialWms.Api.Entities;

public sealed class StockAdjustment
{
    public long AdjustmentId { get; set; }
    public string AdjustmentNumber { get; set; } = null!;
    public int WarehouseId { get; set; }
    public DateTime AdjustmentDate { get; set; }
    public string Reason { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int? CreatedByEmployeeId { get; set; }
}
