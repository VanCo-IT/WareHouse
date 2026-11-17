namespace CommercialWms.Api.Entities;

public sealed class ExchangeRate
{
    public long ExchangeRateId { get; set; }
    public int CurrencyId { get; set; }
    public DateOnly RateDate { get; set; }
    public decimal RateToVND { get; set; }
    public DateTime CreatedAt { get; set; }
}
