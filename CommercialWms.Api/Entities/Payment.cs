namespace CommercialWms.Api.Entities;

public sealed class Payment
{
    public long PaymentId { get; set; }
    public string PaymentNumber { get; set; } = null!;
    public int CustomerId { get; set; }
    public DateTime PaymentDate { get; set; }
    public int CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string Status { get; set; } = null!;
}
