using CommercialWms.Api.Contracts.Common;
using CommercialWms.Api.Contracts.Manufacturers;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ManufacturersController(CommercialWmsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ManufacturerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ManufacturerResponse>>> GetManufacturers(
        [FromQuery] CatalogListQuery query,
        CancellationToken cancellationToken)
    {
        var manufacturers = dbContext.Manufacturers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            manufacturers = manufacturers.Where(manufacturer =>
                manufacturer.ManufacturerCode.Contains(search)
                || manufacturer.ManufacturerName.Contains(search));
        }

        if (query.IsActive.HasValue)
        {
            manufacturers = manufacturers.Where(manufacturer =>
                manufacturer.IsActive == query.IsActive.Value);
        }

        var totalCount = await manufacturers.CountAsync(cancellationToken);
        var result = await manufacturers
            .OrderBy(manufacturer => manufacturer.ManufacturerName)
            .ApplyCatalogPaging(query, totalCount, Response.Headers)
            .Select(manufacturer => ToResponse(manufacturer))
            .ToListAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ManufacturerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ManufacturerResponse>> GetManufacturer(
        int id,
        CancellationToken cancellationToken)
    {
        var manufacturer = await dbContext.Manufacturers
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.ManufacturerId == id, cancellationToken);

        return manufacturer is null ? NotFound() : Ok(ToResponse(manufacturer));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ManufacturerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ManufacturerResponse>> CreateManufacturer(
        ManufacturerCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Countries.AnyAsync(
                country => country.CountryId == request.CountryId,
                cancellationToken))
        {
            return CatalogControllerHelpers.InvalidReference(
                $"Country {request.CountryId} was not found.");
        }

        var code = request.ManufacturerCode.Trim();
        if (await dbContext.Manufacturers.AnyAsync(
                manufacturer => manufacturer.ManufacturerCode == code,
                cancellationToken))
        {
            return ManufacturerConflict();
        }

        var manufacturer = new Manufacturer
        {
            ManufacturerCode = code,
            ManufacturerName = request.ManufacturerName.Trim(),
            CountryId = request.CountryId,
            Address = NormalizeOptional(request.Address),
            IsActive = request.IsActive ?? true
        };

        dbContext.Manufacturers.Add(manufacturer);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_Manufacturers_ManufacturerCode"))
        {
            return ManufacturerConflict();
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return CatalogControllerHelpers.InvalidReference(
                "The specified country does not exist.");
        }

        return CreatedAtAction(
            nameof(GetManufacturer),
            new { id = manufacturer.ManufacturerId },
            ToResponse(manufacturer));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ManufacturerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ManufacturerResponse>> UpdateManufacturer(
        int id,
        ManufacturerUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var manufacturer = await dbContext.Manufacturers
            .SingleOrDefaultAsync(item => item.ManufacturerId == id, cancellationToken);
        if (manufacturer is null)
        {
            return NotFound();
        }

        if (!await dbContext.Countries.AnyAsync(
                country => country.CountryId == request.CountryId,
                cancellationToken))
        {
            return CatalogControllerHelpers.InvalidReference(
                $"Country {request.CountryId} was not found.");
        }

        var code = request.ManufacturerCode.Trim();
        if (await dbContext.Manufacturers.AnyAsync(
                existing => existing.ManufacturerId != id
                    && existing.ManufacturerCode == code,
                cancellationToken))
        {
            return ManufacturerConflict();
        }

        manufacturer.ManufacturerCode = code;
        manufacturer.ManufacturerName = request.ManufacturerName.Trim();
        manufacturer.CountryId = request.CountryId;
        manufacturer.Address = NormalizeOptional(request.Address);
        manufacturer.IsActive = request.IsActive;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_Manufacturers_ManufacturerCode"))
        {
            return ManufacturerConflict();
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return CatalogControllerHelpers.InvalidReference(
                "The specified country does not exist.");
        }

        return Ok(ToResponse(manufacturer));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteManufacturer(int id, CancellationToken cancellationToken)
    {
        var manufacturer = await dbContext.Manufacturers
            .SingleOrDefaultAsync(item => item.ManufacturerId == id, cancellationToken);
        if (manufacturer is null)
        {
            return NotFound();
        }

        if (await IsReferencedAsync(id, cancellationToken))
        {
            return ManufacturerInUseConflict();
        }

        dbContext.Manufacturers.Remove(manufacturer);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return ManufacturerInUseConflict();
        }

        return NoContent();
    }

    private async Task<bool> IsReferencedAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.ManufacturerDocuments.AnyAsync(
                item => item.ManufacturerId == id,
                cancellationToken)
            || await dbContext.FoodSafetyDocuments.AnyAsync(
                item => item.ManufacturerId == id,
                cancellationToken)
            || await dbContext.SupplierProducts.AnyAsync(
                item => item.ManufacturerId == id,
                cancellationToken)
            || await dbContext.InventoryLots.AnyAsync(
                item => item.ManufacturerId == id,
                cancellationToken);
    }

    private static ManufacturerResponse ToResponse(Manufacturer manufacturer)
    {
        return new ManufacturerResponse(
            manufacturer.ManufacturerId,
            manufacturer.ManufacturerCode,
            manufacturer.ManufacturerName,
            manufacturer.CountryId,
            manufacturer.Address,
            manufacturer.IsActive);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private ConflictObjectResult ManufacturerConflict()
    {
        return CatalogControllerHelpers.InUseConflict(
            "A manufacturer with this code already exists.");
    }

    private ConflictObjectResult ManufacturerInUseConflict()
    {
        return CatalogControllerHelpers.InUseConflict(
            "The manufacturer cannot be deleted because it is referenced by documents, supplier products, or lots.");
    }
}
