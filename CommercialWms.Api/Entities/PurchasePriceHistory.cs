namespace CommercialWms.Api.Entities;

public sealed class PurchasePriceHistory
{
    public long PurchasePriceHistoryId { get; set; }
    public int ProductId { get; set; }
    public int SupplierId { get; set; }
    public long PurchaseOrderDetailId { get; set; }
    public int CurrencyId { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime RecordedAt { get; set; }
}
