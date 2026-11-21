using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Countries;

/// <summary>Request to update a country.</summary>
public sealed record CountryUpdateRequest
{
    [Required, RegularExpression("^[A-Za-z]{2}$")]
    public string CountryCode { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string CountryName { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}
