namespace CommercialWms.Api.Entities;

public sealed class Product
{
    public int ProductId { get; set; }
    public string SKU { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public int CategoryId { get; set; }
    public int? BrandId { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public bool IsPerishable { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
