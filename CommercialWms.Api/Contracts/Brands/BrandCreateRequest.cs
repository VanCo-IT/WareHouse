using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Brands;

public sealed class BrandCreateRequest
{
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string BrandName { get; init; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int CountryId { get; init; }

    public bool? IsActive { get; init; }
}
