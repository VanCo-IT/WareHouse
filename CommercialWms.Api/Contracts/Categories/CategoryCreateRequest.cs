using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Categories;

public sealed class CategoryCreateRequest
{
    [Required]
    [StringLength(30, MinimumLength = 1)]
    public string CategoryCode { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string CategoryName { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int? ParentCategoryId { get; init; }

    public bool? IsActive { get; init; }
}
