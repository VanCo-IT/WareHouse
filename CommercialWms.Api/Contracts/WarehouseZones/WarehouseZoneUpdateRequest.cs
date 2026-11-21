using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.WarehouseZones;

/// <summary>Payload for updating a warehouse zone.</summary>
public sealed record WarehouseZoneUpdateRequest
{
    [Range(1, int.MaxValue)]
    public int WarehouseId { get; init; }

    [Required, StringLength(30, MinimumLength = 1)]
    public string ZoneCode { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string ZoneName { get; init; } = string.Empty;
}
