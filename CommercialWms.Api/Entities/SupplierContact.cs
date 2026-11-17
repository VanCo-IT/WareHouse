namespace CommercialWms.Api.Entities;

public sealed class SupplierContact
{
    public int ContactId { get; set; }
    public int SupplierId { get; set; }
    public string ContactName { get; set; } = null!;
    public string? JobTitle { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
