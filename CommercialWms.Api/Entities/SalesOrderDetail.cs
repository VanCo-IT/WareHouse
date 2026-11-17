namespace CommercialWms.Api.Entities;

public sealed class SalesOrderDetail
{
    public long SalesOrderDetailId { get; set; }
    public long SalesOrderId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public int UnitId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
}
