using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Suppliers;

public sealed class SupplierStatusUpdateRequest
{
    [Required]
    public bool? IsActive { get; init; }
}
