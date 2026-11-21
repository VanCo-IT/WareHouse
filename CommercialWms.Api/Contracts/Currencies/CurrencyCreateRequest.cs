using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Currencies;

/// <summary>Request to create a currency.</summary>
public sealed record CurrencyCreateRequest
{
    [Required, RegularExpression("^[A-Za-z]{3}$")]
    public string CurrencyCode { get; init; } = string.Empty;

    [Required, StringLength(80, MinimumLength = 1)]
    public string CurrencyName { get; init; } = string.Empty;

    public bool? IsActive { get; init; }
}
