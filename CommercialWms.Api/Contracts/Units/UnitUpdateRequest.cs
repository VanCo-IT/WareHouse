using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Units;

/// <summary>Request to update a unit of measure.</summary>
public sealed record UnitUpdateRequest
{
    [Required, StringLength(20, MinimumLength = 1)]
    public string UnitCode { get; init; } = string.Empty;

    [Required, StringLength(60, MinimumLength = 1)]
    public string UnitName { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}
