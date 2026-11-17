namespace CommercialWms.Api.Entities;

public sealed class Currency
{
    public int CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public string CurrencyName { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}
