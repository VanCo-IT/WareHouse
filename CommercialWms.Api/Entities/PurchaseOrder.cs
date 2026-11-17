namespace CommercialWms.Api.Entities;

public sealed class PurchaseOrder
{
    public long PurchaseOrderId { get; set; }
    public string PONumber { get; set; } = null!;
    public int SupplierId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateOnly? ExpectedDate { get; set; }
    public int CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; }
    public string Status { get; set; } = null!;
    public int? CreatedByEmployeeId { get; set; }
    public string? Note { get; set; }
}
