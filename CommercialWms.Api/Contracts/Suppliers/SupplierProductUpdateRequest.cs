using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Suppliers;

public sealed class SupplierProductUpdateRequest
{
    [StringLength(60)]
    public string? SupplierProductCode { get; init; }

    [Range(0, int.MaxValue)]
    public int LeadTimeDays { get; init; }

    [Required]
    public bool? IsActive { get; init; }
}
