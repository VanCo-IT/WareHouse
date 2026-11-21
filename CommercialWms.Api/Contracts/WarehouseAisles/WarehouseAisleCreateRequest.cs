using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.WarehouseAisles;

/// <summary>Payload for creating a warehouse aisle.</summary>
public sealed record WarehouseAisleCreateRequest
{
    [Range(1, int.MaxValue)]
    public int ZoneId { get; init; }

    [Required, StringLength(30, MinimumLength = 1)]
    public string AisleCode { get; init; } = string.Empty;
}
