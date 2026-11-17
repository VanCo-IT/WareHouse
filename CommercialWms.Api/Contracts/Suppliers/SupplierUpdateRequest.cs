using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Suppliers;

public sealed class SupplierUpdateRequest
{
    [Required]
    [StringLength(30, MinimumLength = 1)]
    public string SupplierCode { get; init; } = string.Empty;

    [Required]
    [StringLength(180, MinimumLength = 1)]
    public string SupplierName { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CountryId { get; init; }

    [StringLength(40)]
    public string? TaxCode { get; init; }

    [StringLength(150)]
    public string? Email { get; init; }

    [StringLength(30)]
    public string? Phone { get; init; }

    [StringLength(300)]
    public string? Address { get; init; }

    [Required]
    public bool? IsActive { get; init; }
}
