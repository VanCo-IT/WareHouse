using System.Globalization;
using CommercialWms.Api.Contracts.Common;
using Microsoft.AspNetCore.Http;

namespace CommercialWms.Api.Data;

public static class CatalogQueryExtensions
{
    public static IQueryable<T> ApplyCatalogPaging<T>(
        this IQueryable<T> query,
        CatalogListQuery options,
        int totalCount,
        IHeaderDictionary responseHeaders)
    {
        responseHeaders["X-Total-Count"] = totalCount.ToString(CultureInfo.InvariantCulture);

        if (!options.Page.HasValue && !options.PageSize.HasValue)
        {
            return query;
        }

        var page = options.Page ?? 1;
        var pageSize = options.PageSize ?? 50;
        var offset = (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        responseHeaders["X-Page"] = page.ToString(CultureInfo.InvariantCulture);
        responseHeaders["X-Page-Size"] = pageSize.ToString(CultureInfo.InvariantCulture);
        responseHeaders["X-Total-Pages"] = totalPages.ToString(CultureInfo.InvariantCulture);

        return query.Skip(offset).Take(pageSize);
    }
}
