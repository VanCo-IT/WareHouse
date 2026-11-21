using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.WarehouseRacks;

/// <summary>Payload for creating a warehouse rack.</summary>
public sealed record WarehouseRackCreateRequest
{
    [Range(1, int.MaxValue)]
    public int AisleId { get; init; }

    [Required, StringLength(30, MinimumLength = 1)]
    public string RackCode { get; init; } = string.Empty;
}
