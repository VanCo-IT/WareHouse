using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.WarehouseShelves;

/// <summary>Payload for updating a warehouse shelf.</summary>
public sealed record WarehouseShelfUpdateRequest
{
    [Range(1, int.MaxValue)]
    public int RackId { get; init; }

    [Required, StringLength(30, MinimumLength = 1)]
    public string ShelfCode { get; init; } = string.Empty;
}
