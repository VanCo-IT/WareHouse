using CommercialWms.Api.Contracts.Common;
using CommercialWms.Api.Contracts.Countries;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CountriesController(CommercialWmsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CountryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CountryResponse>>> GetCountries(
        [FromQuery] CatalogListQuery query,
        CancellationToken cancellationToken)
    {
        var countries = dbContext.Countries.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            countries = countries.Where(country =>
                country.CountryCode.Contains(search) || country.CountryName.Contains(search));
        }

        if (query.IsActive.HasValue)
        {
            countries = countries.Where(country => country.IsActive == query.IsActive.Value);
        }

        var totalCount = await countries.CountAsync(cancellationToken);
        var result = await countries
            .OrderBy(country => country.CountryName)
            .ApplyCatalogPaging(query, totalCount, Response.Headers)
            .Select(country => ToResponse(country))
            .ToListAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CountryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CountryResponse>> GetCountry(int id, CancellationToken cancellationToken)
    {
        var country = await dbContext.Countries
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.CountryId == id, cancellationToken);

        return country is null ? NotFound() : Ok(ToResponse(country));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CountryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CountryResponse>> CreateCountry(
        CountryCreateRequest request,
        CancellationToken cancellationToken)
    {
        var code = request.CountryCode.Trim().ToUpperInvariant();
        var name = request.CountryName.Trim();
        if (await HasDuplicateAsync(code, name, null, cancellationToken))
        {
            return CountryConflict();
        }

        var country = new Country
        {
            CountryCode = code,
            CountryName = name,
            IsActive = request.IsActive ?? true
        };

        dbContext.Countries.Add(country);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(
                exception,
                "IX_Countries_CountryCode",
                "IX_Countries_CountryName"))
        {
            return CountryConflict();
        }

        return CreatedAtAction(
            nameof(GetCountry),
            new { id = country.CountryId },
            ToResponse(country));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CountryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CountryResponse>> UpdateCountry(
        int id,
        CountryUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var country = await dbContext.Countries
            .SingleOrDefaultAsync(item => item.CountryId == id, cancellationToken);
        if (country is null)
        {
            return NotFound();
        }

        var code = request.CountryCode.Trim().ToUpperInvariant();
        var name = request.CountryName.Trim();
        if (await HasDuplicateAsync(code, name, id, cancellationToken))
        {
            return CountryConflict();
        }

        country.CountryCode = code;
        country.CountryName = name;
        country.IsActive = request.IsActive;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(
                exception,
                "IX_Countries_CountryCode",
                "IX_Countries_CountryName"))
        {
            return CountryConflict();
        }

        return Ok(ToResponse(country));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCountry(int id, CancellationToken cancellationToken)
    {
        var country = await dbContext.Countries
            .SingleOrDefaultAsync(item => item.CountryId == id, cancellationToken);
        if (country is null)
        {
            return NotFound();
        }

        if (await IsReferencedAsync(id, cancellationToken))
        {
            return CountryInUseConflict();
        }

        dbContext.Countries.Remove(country);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return CountryInUseConflict();
        }

        return NoContent();
    }

    private async Task<bool> HasDuplicateAsync(
        string code,
        string name,
        int? excludedId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Countries.AnyAsync(
            country => country.CountryId != excludedId
                && (country.CountryCode == code || country.CountryName == name),
            cancellationToken);
    }

    private async Task<bool> IsReferencedAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.Brands.AnyAsync(item => item.CountryId == id, cancellationToken)
            || await dbContext.Suppliers.AnyAsync(item => item.CountryId == id, cancellationToken)
            || await dbContext.Manufacturers.AnyAsync(item => item.CountryId == id, cancellationToken)
            || await dbContext.InventoryLots.AnyAsync(item => item.CountryId == id, cancellationToken);
    }

    private static CountryResponse ToResponse(Country country)
    {
        return new CountryResponse(
            country.CountryId,
            country.CountryCode,
            country.CountryName,
            country.IsActive);
    }

    private ConflictObjectResult CountryConflict()
    {
        return CatalogControllerHelpers.InUseConflict(
            "A country with this code or name already exists.");
    }

    private ConflictObjectResult CountryInUseConflict()
    {
        return CatalogControllerHelpers.InUseConflict(
            "The country cannot be deleted because it is referenced by other records.");
    }
}
