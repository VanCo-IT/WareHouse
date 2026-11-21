using Microsoft.AspNetCore.Mvc;

namespace CommercialWms.Api.Controllers;

internal static class WarehouseControllerHelpers
{
    public static bool HasRequiredText(params string?[] values)
    {
        return values.All(value => !string.IsNullOrWhiteSpace(value));
    }

    public static BadRequestObjectResult InvalidInput(string detail)
    {
        return new BadRequestObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid warehouse input",
            Detail = detail
        });
    }

    public static BadRequestObjectResult InvalidReference(string detail)
    {
        return new BadRequestObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid warehouse reference",
            Detail = detail
        });
    }

    public static ConflictObjectResult UniqueConflict(string detail)
    {
        return new ConflictObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Detail = detail
        });
    }

    public static ConflictObjectResult InUseConflict(string detail)
    {
        return new ConflictObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Location is in use",
            Detail = detail
        });
    }
}
