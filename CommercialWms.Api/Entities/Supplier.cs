namespace CommercialWms.Api.Entities;

public sealed class Supplier
{
    public int SupplierId { get; set; }
    public string SupplierCode { get; set; } = null!;
    public string SupplierName { get; set; } = null!;
    public int CountryId { get; set; }
    public string? TaxCode { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}
