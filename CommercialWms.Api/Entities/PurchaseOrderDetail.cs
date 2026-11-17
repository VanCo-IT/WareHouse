namespace CommercialWms.Api.Entities;

public sealed class PurchaseOrderDetail
{
    public long PurchaseOrderDetailId { get; set; }
    public long PurchaseOrderId { get; set; }
    public int ProductId { get; set; }
    public decimal OrderedQuantity { get; set; }
    public int UnitId { get; set; }
    public decimal UnitPrice { get; set; }
}
