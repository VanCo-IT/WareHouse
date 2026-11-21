using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.WarehouseBins;

/// <summary>Payload for updating a warehouse bin.</summary>
public sealed record WarehouseBinUpdateRequest
{
    [Range(1, int.MaxValue)]
    public int ShelfId { get; init; }

    [Required, StringLength(30, MinimumLength = 1)]
    public string BinCode { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.001", "999999999999999.999")]
    public decimal? Capacity { get; init; }

    public bool IsActive { get; init; }
}
