using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Common;

/// <summary>Optional search, status, and pagination parameters for a catalog list.</summary>
public sealed record CatalogListQuery
{
    /// <summary>Filters the list by a resource-specific code or name.</summary>
    [StringLength(100)]
    public string? Search { get; init; }

    /// <summary>Filters by active status when supplied.</summary>
    public bool? IsActive { get; init; }

    /// <summary>One-based page number. Pagination is omitted when both page fields are absent.</summary>
    [Range(1, int.MaxValue)]
    public int? Page { get; init; }

    /// <summary>Page size from 1 through 100.</summary>
    [Range(1, 100)]
    public int? PageSize { get; init; }
}
