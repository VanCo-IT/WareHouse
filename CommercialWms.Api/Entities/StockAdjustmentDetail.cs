namespace CommercialWms.Api.Entities;

public sealed class StockAdjustmentDetail
{
    public long AdjustmentDetailId { get; set; }
    public long AdjustmentId { get; set; }
    public int ProductId { get; set; }
    public long LotId { get; set; }
    public int BinId { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal? DifferenceQuantity { get; set; }
}
