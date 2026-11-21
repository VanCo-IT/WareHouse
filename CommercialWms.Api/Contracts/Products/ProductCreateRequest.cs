using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Products;

public sealed class ProductCreateRequest
{
    [Required]
    [StringLength(40, MinimumLength = 1)]
    public string SKU { get; init; } = string.Empty;

    [Required]
    [StringLength(180, MinimumLength = 1)]
    public string ProductName { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }

    [Range(1, int.MaxValue)]
    public int UnitId { get; init; }

    [Range(1, int.MaxValue)]
    public int? BrandId { get; init; }

    [StringLength(60)]
    public string? Barcode { get; init; }

    [StringLength(500)]
    public string? Description { get; init; }

    public bool? IsPerishable { get; init; }

    public bool? IsActive { get; init; }
}
