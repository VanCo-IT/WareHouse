namespace CommercialWms.Api.Entities;

public sealed class GoodsReceiptDetail
{
    public long GoodsReceiptDetailId { get; set; }
    public long GoodsReceiptId { get; set; }
    public long PurchaseOrderDetailId { get; set; }
    public int ProductId { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public int UnitId { get; set; }
}
