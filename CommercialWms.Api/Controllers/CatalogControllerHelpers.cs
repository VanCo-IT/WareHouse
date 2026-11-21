using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

internal static class CatalogControllerHelpers
{
    public static bool IsUniqueViolation(DbUpdateException exception, params string[] indexNames)
    {
        return exception.GetBaseException() is SqlException { Number: 2601 or 2627 } sqlException
            && indexNames.Any(indexName =>
                sqlException.Message.Contains(indexName, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsForeignKeyViolation(DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException { Number: 547 };
    }

    public static ConflictObjectResult InUseConflict(string detail)
    {
        return new ConflictObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Detail = detail
        });
    }

    public static BadRequestObjectResult InvalidReference(string detail)
    {
        return new BadRequestObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid reference",
            Detail = detail
        });
    }
}
