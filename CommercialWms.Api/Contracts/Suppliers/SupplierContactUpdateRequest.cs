using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Suppliers;

public sealed class SupplierContactUpdateRequest
{
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string ContactName { get; init; } = string.Empty;

    [StringLength(80)]
    public string? JobTitle { get; init; }

    [StringLength(150)]
    public string? Email { get; init; }

    [StringLength(30)]
    public string? Phone { get; init; }
}
