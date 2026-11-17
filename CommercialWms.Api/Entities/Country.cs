namespace CommercialWms.Api.Entities;

public sealed class Country
{
    public int CountryId { get; set; }
    public string CountryCode { get; set; } = null!;
    public string CountryName { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}
