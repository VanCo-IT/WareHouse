using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Manufacturers;

/// <summary>Request to create a manufacturer.</summary>
public sealed record ManufacturerCreateRequest
{
    [Required, StringLength(30, MinimumLength = 1)]
    public string ManufacturerCode { get; init; } = string.Empty;

    [Required, StringLength(180, MinimumLength = 1)]
    public string ManufacturerName { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CountryId { get; init; }

    [StringLength(300)]
    public string? Address { get; init; }

    public bool? IsActive { get; init; }
}
