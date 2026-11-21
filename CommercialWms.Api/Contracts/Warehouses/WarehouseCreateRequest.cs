using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Warehouses;

/// <summary>Payload for creating a warehouse.</summary>
public sealed record WarehouseCreateRequest
{
    [Required, StringLength(30, MinimumLength = 1)]
    public string WarehouseCode { get; init; } = string.Empty;

    [Required, StringLength(150, MinimumLength = 1)]
    public string WarehouseName { get; init; } = string.Empty;

    [Required, StringLength(300, MinimumLength = 1)]
    public string Address { get; init; } = string.Empty;

    public bool? IsActive { get; init; }
}
