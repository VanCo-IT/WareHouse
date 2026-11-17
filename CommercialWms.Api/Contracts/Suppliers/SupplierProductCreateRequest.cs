using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Suppliers;

public sealed class SupplierProductCreateRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; init; }

    [Range(1, int.MaxValue)]
    public int ManufacturerId { get; init; }

    [StringLength(60)]
    public string? SupplierProductCode { get; init; }

    [Range(0, int.MaxValue)]
    public int LeadTimeDays { get; init; }

    public bool? IsActive { get; init; }
}
