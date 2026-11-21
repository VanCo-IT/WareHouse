using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.WarehouseRacks;

/// <summary>Payload for updating a warehouse rack.</summary>
public sealed record WarehouseRackUpdateRequest
{
    [Range(1, int.MaxValue)]
    public int AisleId { get; init; }

    [Required, StringLength(30, MinimumLength = 1)]
    public string RackCode { get; init; } = string.Empty;
}
