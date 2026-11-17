namespace CommercialWms.Api.Entities;

public sealed class ProductCategory
{
    public int CategoryId { get; set; }
    public int? ParentCategoryId { get; set; }
    public string CategoryCode { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}
