using CommercialWms.Api.Contracts.Categories;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController(CommercialWmsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await dbContext.ProductCategories
            .AsNoTracking()
            .OrderBy(category => category.CategoryId)
            .Select(category => ToResponse(category))
            .ToListAsync(cancellationToken);

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> GetCategory(int id, CancellationToken cancellationToken)
    {
        var category = await dbContext.ProductCategories
            .AsNoTracking()
            .Where(category => category.CategoryId == id)
            .Select(category => ToResponse(category))
            .SingleOrDefaultAsync(cancellationToken);

        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryResponse>> CreateCategory(
        CategoryCreateRequest request,
        CancellationToken cancellationToken)
    {
        var code = request.CategoryCode.Trim();
        var parentError = await ValidateParentAsync(request.ParentCategoryId, null, cancellationToken);
        if (parentError is not null)
        {
            return BadRequest(CreateProblem(StatusCodes.Status400BadRequest, parentError));
        }

        if (await dbContext.ProductCategories.AnyAsync(
                category => category.CategoryCode == code,
                cancellationToken))
        {
            return CategoryCodeConflict();
        }

        var category = new ProductCategory
        {
            CategoryCode = code,
            CategoryName = request.CategoryName.Trim(),
            ParentCategoryId = request.ParentCategoryId,
            IsActive = request.IsActive ?? true
        };

        dbContext.ProductCategories.Add(category);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsCategoryCodeUniqueViolation(exception))
        {
            return CategoryCodeConflict();
        }

        var response = ToResponse(category);
        return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryId }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(
        int id,
        CategoryUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var category = await dbContext.ProductCategories
            .SingleOrDefaultAsync(item => item.CategoryId == id, cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        var parentError = await ValidateParentAsync(request.ParentCategoryId, id, cancellationToken);
        if (parentError is not null)
        {
            return BadRequest(CreateProblem(StatusCodes.Status400BadRequest, parentError));
        }

        var code = request.CategoryCode.Trim();
        if (await dbContext.ProductCategories.AnyAsync(
                existing => existing.CategoryId != id && existing.CategoryCode == code,
                cancellationToken))
        {
            return CategoryCodeConflict();
        }

        category.CategoryCode = code;
        category.CategoryName = request.CategoryName.Trim();
        category.ParentCategoryId = request.ParentCategoryId;
        category.IsActive = request.IsActive!.Value;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsCategoryCodeUniqueViolation(exception))
        {
            return CategoryCodeConflict();
        }

        return Ok(ToResponse(category));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        var category = await dbContext.ProductCategories
            .SingleOrDefaultAsync(item => item.CategoryId == id, cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        if (await IsCategoryReferencedAsync(id, cancellationToken))
        {
            return CategoryInUseConflict();
        }

        dbContext.ProductCategories.Remove(category);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            return CategoryInUseConflict();
        }

        return NoContent();
    }

    private async Task<string?> ValidateParentAsync(
        int? parentCategoryId,
        int? categoryId,
        CancellationToken cancellationToken)
    {
        if (!parentCategoryId.HasValue)
        {
            return null;
        }

        if (categoryId == parentCategoryId)
        {
            return "A category cannot be its own parent.";
        }

        if (!await dbContext.ProductCategories.AnyAsync(
                category => category.CategoryId == parentCategoryId.Value,
                cancellationToken))
        {
            return $"Parent category {parentCategoryId.Value} was not found.";
        }

        return null;
    }

    private async Task<bool> IsCategoryReferencedAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.Products.AnyAsync(product => product.CategoryId == id, cancellationToken)
            || await dbContext.ProductCategories.AnyAsync(category => category.ParentCategoryId == id, cancellationToken);
    }

    private static CategoryResponse ToResponse(ProductCategory category)
    {
        return new CategoryResponse(
            category.CategoryId,
            category.CategoryCode,
            category.CategoryName,
            category.ParentCategoryId,
            category.IsActive);
    }

    private static bool IsCategoryCodeUniqueViolation(DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException { Number: 2601 or 2627 } sqlException
            && sqlException.Message.Contains("IX_ProductCategories_CategoryCode", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsForeignKeyViolation(DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException { Number: 547 };
    }

    private ConflictObjectResult CategoryCodeConflict()
    {
        return Conflict(CreateProblem(StatusCodes.Status409Conflict, "A category with this code already exists."));
    }

    private ConflictObjectResult CategoryInUseConflict()
    {
        return Conflict(CreateProblem(
            StatusCodes.Status409Conflict,
            "The category cannot be deleted because it is referenced by products or child categories."));
    }

    private static ProblemDetails CreateProblem(int status, string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = status == StatusCodes.Status409Conflict ? "Conflict" : "Invalid category reference",
            Detail = detail
        };
    }
}
