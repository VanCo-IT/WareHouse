namespace CommercialWms.Api.Entities;

public sealed class Manufacturer
{
    public int ManufacturerId { get; set; }
    public string ManufacturerCode { get; set; } = null!;
    public string ManufacturerName { get; set; } = null!;
    public int CountryId { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}
