namespace CommercialWms.Api.Entities;

public sealed class DeliveryDetail
{
    public long DeliveryDetailId { get; set; }
    public long DeliveryId { get; set; }
    public long SalesOrderDetailId { get; set; }
    public long LotId { get; set; }
    public decimal Quantity { get; set; }
}
