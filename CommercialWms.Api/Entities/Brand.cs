namespace CommercialWms.Api.Entities;

public sealed class Brand
{
    public int BrandId { get; set; }
    public string BrandName { get; set; } = null!;
    public int CountryId { get; set; }
    public bool IsActive { get; set; } = true;
}
