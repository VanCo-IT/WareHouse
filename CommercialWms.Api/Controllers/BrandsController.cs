using CommercialWms.Api.Contracts.Brands;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BrandsController(CommercialWmsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BrandResponse>>> GetBrands(CancellationToken cancellationToken)
    {
        var brands = await dbContext.Brands
            .AsNoTracking()
            .OrderBy(brand => brand.BrandId)
            .Select(brand => ToResponse(brand))
            .ToListAsync(cancellationToken);

        return Ok(brands);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrandResponse>> GetBrand(int id, CancellationToken cancellationToken)
    {
        var brand = await dbContext.Brands
            .AsNoTracking()
            .Where(item => item.BrandId == id)
            .Select(item => ToResponse(item))
            .SingleOrDefaultAsync(cancellationToken);

        return brand is null ? NotFound() : Ok(brand);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BrandResponse>> CreateBrand(
        [FromBody] BrandCreateRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.BrandName.Trim();
        if (!await dbContext.Countries.AnyAsync(country => country.CountryId == request.CountryId, cancellationToken))
        {
            return BadRequest(CreateProblem(StatusCodes.Status400BadRequest, $"Country {request.CountryId} was not found."));
        }

        if (await HasDuplicateNameAsync(name, request.CountryId, null, cancellationToken))
        {
            return BrandNameConflict();
        }

        var brand = new Brand
        {
            BrandName = name,
            CountryId = request.CountryId,
            IsActive = request.IsActive ?? true
        };

        dbContext.Brands.Add(brand);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsBrandNameUniqueViolation(exception))
        {
            return BrandNameConflict();
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            return BadRequest(CreateProblem(StatusCodes.Status400BadRequest, "The specified country does not exist."));
        }

        var response = ToResponse(brand);
        return CreatedAtAction(nameof(GetBrand), new { id = brand.BrandId }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BrandResponse>> UpdateBrand(
        int id,
        [FromBody] BrandUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var brand = await dbContext.Brands
            .SingleOrDefaultAsync(item => item.BrandId == id, cancellationToken);
        if (brand is null)
        {
            return NotFound();
        }

        if (!await dbContext.Countries.AnyAsync(country => country.CountryId == request.CountryId, cancellationToken))
        {
            return BadRequest(CreateProblem(StatusCodes.Status400BadRequest, $"Country {request.CountryId} was not found."));
        }

        var name = request.BrandName.Trim();
        if (await HasDuplicateNameAsync(name, request.CountryId, id, cancellationToken))
        {
            return BrandNameConflict();
        }

        brand.BrandName = name;
        brand.CountryId = request.CountryId;
        brand.IsActive = request.IsActive!.Value;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsBrandNameUniqueViolation(exception))
        {
            return BrandNameConflict();
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            return BadRequest(CreateProblem(StatusCodes.Status400BadRequest, "The specified country does not exist."));
        }

        return Ok(ToResponse(brand));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteBrand(int id, CancellationToken cancellationToken)
    {
        var brand = await dbContext.Brands
            .SingleOrDefaultAsync(item => item.BrandId == id, cancellationToken);
        if (brand is null)
        {
            return NotFound();
        }

        if (await dbContext.Products.AnyAsync(product => product.BrandId == id, cancellationToken))
        {
            return BrandInUseConflict();
        }

        dbContext.Brands.Remove(brand);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            return BrandInUseConflict();
        }

        return NoContent();
    }

    private async Task<bool> HasDuplicateNameAsync(
        string brandName,
        int countryId,
        int? excludedBrandId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Brands.AnyAsync(
            brand => brand.CountryId == countryId
                && brand.BrandName == brandName
                && (!excludedBrandId.HasValue || brand.BrandId != excludedBrandId.Value),
            cancellationToken);
    }

    private static BrandResponse ToResponse(Brand brand)
    {
        return new BrandResponse(brand.BrandId, brand.BrandName, brand.CountryId, brand.IsActive);
    }

    private static bool IsBrandNameUniqueViolation(DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException { Number: 2601 or 2627 } sqlException
            && sqlException.Message.Contains("IX_Brands_BrandName_CountryId", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsForeignKeyViolation(DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException { Number: 547 };
    }

    private ConflictObjectResult BrandNameConflict()
    {
        return Conflict(CreateProblem(
            StatusCodes.Status409Conflict,
            "A brand with this name and country already exists."));
    }

    private ConflictObjectResult BrandInUseConflict()
    {
        return Conflict(CreateProblem(
            StatusCodes.Status409Conflict,
            "The brand cannot be deleted because it is referenced by one or more products."));
    }

    private static ProblemDetails CreateProblem(int status, string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = status == StatusCodes.Status409Conflict ? "Conflict" : "Invalid brand request",
            Detail = detail
        };
    }
}
