using CommercialWms.Api.Contracts.Common;
using CommercialWms.Api.Contracts.Units;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UnitsController(CommercialWmsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UnitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UnitResponse>>> GetUnits(
        [FromQuery] CatalogListQuery query,
        CancellationToken cancellationToken)
    {
        var units = dbContext.Units.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            units = units.Where(unit => unit.UnitCode.Contains(search) || unit.UnitName.Contains(search));
        }

        if (query.IsActive.HasValue)
        {
            units = units.Where(unit => unit.IsActive == query.IsActive.Value);
        }

        var totalCount = await units.CountAsync(cancellationToken);
        var result = await units
            .OrderBy(unit => unit.UnitName)
            .ApplyCatalogPaging(query, totalCount, Response.Headers)
            .Select(unit => ToResponse(unit))
            .ToListAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UnitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UnitResponse>> GetUnit(int id, CancellationToken cancellationToken)
    {
        var unit = await dbContext.Units
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.UnitId == id, cancellationToken);

        return unit is null ? NotFound() : Ok(ToResponse(unit));
    }

    [HttpPost]
    [ProducesResponseType(typeof(UnitResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UnitResponse>> CreateUnit(
        UnitCreateRequest request,
        CancellationToken cancellationToken)
    {
        var code = request.UnitCode.Trim();
        if (await dbContext.Units.AnyAsync(item => item.UnitCode == code, cancellationToken))
        {
            return UnitConflict();
        }

        var unit = new Unit
        {
            UnitCode = code,
            UnitName = request.UnitName.Trim(),
            IsActive = request.IsActive ?? true
        };

        dbContext.Units.Add(unit);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_Units_UnitCode"))
        {
            return UnitConflict();
        }

        return CreatedAtAction(nameof(GetUnit), new { id = unit.UnitId }, ToResponse(unit));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UnitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UnitResponse>> UpdateUnit(
        int id,
        UnitUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var unit = await dbContext.Units
            .SingleOrDefaultAsync(item => item.UnitId == id, cancellationToken);
        if (unit is null)
        {
            return NotFound();
        }

        var code = request.UnitCode.Trim();
        if (await dbContext.Units.AnyAsync(
                item => item.UnitId != id && item.UnitCode == code,
                cancellationToken))
        {
            return UnitConflict();
        }

        unit.UnitCode = code;
        unit.UnitName = request.UnitName.Trim();
        unit.IsActive = request.IsActive;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_Units_UnitCode"))
        {
            return UnitConflict();
        }

        return Ok(ToResponse(unit));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteUnit(int id, CancellationToken cancellationToken)
    {
        var unit = await dbContext.Units
            .SingleOrDefaultAsync(item => item.UnitId == id, cancellationToken);
        if (unit is null)
        {
            return NotFound();
        }

        if (await IsReferencedAsync(id, cancellationToken))
        {
            return UnitInUseConflict();
        }

        dbContext.Units.Remove(unit);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return UnitInUseConflict();
        }

        return NoContent();
    }

    private async Task<bool> IsReferencedAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.ProductUnits.AnyAsync(item => item.UnitId == id, cancellationToken)
            || await dbContext.PurchaseOrderDetails.AnyAsync(item => item.UnitId == id, cancellationToken)
            || await dbContext.GoodsReceiptDetails.AnyAsync(item => item.UnitId == id, cancellationToken)
            || await dbContext.SalesOrderDetails.AnyAsync(item => item.UnitId == id, cancellationToken);
    }

    private static UnitResponse ToResponse(Unit unit)
    {
        return new UnitResponse(unit.UnitId, unit.UnitCode, unit.UnitName, unit.IsActive);
    }

    private ConflictObjectResult UnitConflict()
    {
        return CatalogControllerHelpers.InUseConflict("A unit with this code already exists.");
    }

    private ConflictObjectResult UnitInUseConflict()
    {
        return CatalogControllerHelpers.InUseConflict(
            "The unit cannot be deleted because it is used by products or business records.");
    }
}
