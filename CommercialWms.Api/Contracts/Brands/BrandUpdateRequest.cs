using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Brands;

public sealed class BrandUpdateRequest
{
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string BrandName { get; init; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int CountryId { get; init; }

    [Required]
    public bool? IsActive { get; init; }
}
