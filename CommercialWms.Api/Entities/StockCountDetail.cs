namespace CommercialWms.Api.Entities;

public sealed class StockCountDetail
{
    public long StockCountDetailId { get; set; }
    public long StockCountId { get; set; }
    public long LotId { get; set; }
    public int BinId { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal? ActualQuantity { get; set; }
    public decimal? DifferenceQuantity { get; set; }
}
