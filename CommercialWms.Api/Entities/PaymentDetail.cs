namespace CommercialWms.Api.Entities;

public sealed class PaymentDetail
{
    public long PaymentDetailId { get; set; }
    public long PaymentId { get; set; }
    public long SalesOrderId { get; set; }
    public decimal AppliedAmount { get; set; }
}
