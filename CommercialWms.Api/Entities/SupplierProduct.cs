namespace CommercialWms.Api.Entities;

public sealed class SupplierProduct
{
    public long SupplierProductId { get; set; }
    public int SupplierId { get; set; }
    public int ProductId { get; set; }
    public int ManufacturerId { get; set; }
    public string? SupplierProductCode { get; set; }
    public int LeadTimeDays { get; set; }
    public bool IsActive { get; set; } = true;
}
