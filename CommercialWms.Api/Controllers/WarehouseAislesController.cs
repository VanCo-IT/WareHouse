using System.ComponentModel.DataAnnotations;
using CommercialWms.Api.Contracts.Common;
using CommercialWms.Api.Contracts.WarehouseAisles;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WarehouseAislesController(CommercialWmsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseAisleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WarehouseAisleResponse>>> GetAisles(
        [FromQuery] CatalogListQuery query,
        [FromQuery, Range(1, int.MaxValue)] int? zoneId,
        CancellationToken cancellationToken)
    {
        if (zoneId.HasValue && !await dbContext.WarehouseZones.AnyAsync(
                item => item.ZoneId == zoneId.Value,
                cancellationToken))
        {
            return WarehouseControllerHelpers.InvalidReference($"Zone {zoneId.Value} was not found.");
        }

        var aisles = dbContext.WarehouseAisles.AsNoTracking();
        if (zoneId.HasValue)
        {
            aisles = aisles.Where(item => item.ZoneId == zoneId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            aisles = aisles.Where(item => item.AisleCode.Contains(search));
        }

        var totalCount = await aisles.CountAsync(cancellationToken);
        var result = await aisles
            .OrderBy(item => item.AisleCode)
            .ApplyCatalogPaging(query, totalCount, Response.Headers)
            .Select(item => ToResponse(item))
            .ToListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WarehouseAisleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarehouseAisleResponse>> GetAisle(int id, CancellationToken cancellationToken)
    {
        var aisle = await dbContext.WarehouseAisles
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.AisleId == id, cancellationToken);
        return aisle is null ? NotFound() : Ok(ToResponse(aisle));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WarehouseAisleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WarehouseAisleResponse>> CreateAisle(
        WarehouseAisleCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!WarehouseControllerHelpers.HasRequiredText(request.AisleCode))
        {
            return WarehouseControllerHelpers.InvalidInput("Aisle code must not be blank.");
        }

        if (!await dbContext.WarehouseZones.AnyAsync(
                item => item.ZoneId == request.ZoneId,
                cancellationToken))
        {
            return WarehouseControllerHelpers.InvalidReference($"Zone {request.ZoneId} was not found.");
        }

        var aisle = new WarehouseAisle
        {
            ZoneId = request.ZoneId,
            AisleCode = request.AisleCode.Trim()
        };
        dbContext.WarehouseAisles.Add(aisle);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_WarehouseAisles_ZoneId_AisleCode"))
        {
            return AisleCodeConflict();
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseControllerHelpers.InvalidReference("The specified zone does not exist.");
        }

        return CreatedAtAction(nameof(GetAisle), new { id = aisle.AisleId }, ToResponse(aisle));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WarehouseAisleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WarehouseAisleResponse>> UpdateAisle(
        int id,
        WarehouseAisleUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var aisle = await dbContext.WarehouseAisles
            .SingleOrDefaultAsync(item => item.AisleId == id, cancellationToken);
        if (aisle is null)
        {
            return NotFound();
        }

        if (!WarehouseControllerHelpers.HasRequiredText(request.AisleCode))
        {
            return WarehouseControllerHelpers.InvalidInput("Aisle code must not be blank.");
        }

        if (!await dbContext.WarehouseZones.AnyAsync(
                item => item.ZoneId == request.ZoneId,
                cancellationToken))
        {
            return WarehouseControllerHelpers.InvalidReference($"Zone {request.ZoneId} was not found.");
        }

        if (aisle.ZoneId != request.ZoneId
            && await dbContext.WarehouseRacks.AnyAsync(item => item.AisleId == id, cancellationToken))
        {
            return WarehouseControllerHelpers.InUseConflict(
                "An aisle with child racks cannot be moved to another zone.");
        }

        aisle.ZoneId = request.ZoneId;
        aisle.AisleCode = request.AisleCode.Trim();
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_WarehouseAisles_ZoneId_AisleCode"))
        {
            return AisleCodeConflict();
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseControllerHelpers.InvalidReference("The specified zone does not exist.");
        }

        return Ok(ToResponse(aisle));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAisle(int id, CancellationToken cancellationToken)
    {
        var aisle = await dbContext.WarehouseAisles
            .SingleOrDefaultAsync(item => item.AisleId == id, cancellationToken);
        if (aisle is null)
        {
            return NotFound();
        }

        if (await dbContext.WarehouseRacks.AnyAsync(item => item.AisleId == id, cancellationToken))
        {
            return WarehouseControllerHelpers.InUseConflict(
                "The aisle cannot be deleted while it contains racks.");
        }

        dbContext.WarehouseAisles.Remove(aisle);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseControllerHelpers.InUseConflict(
                "The aisle is referenced by other records and cannot be deleted.");
        }

        return NoContent();
    }

    private static WarehouseAisleResponse ToResponse(WarehouseAisle aisle)
    {
        return new WarehouseAisleResponse(aisle.AisleId, aisle.ZoneId, aisle.AisleCode);
    }

    private ConflictObjectResult AisleCodeConflict()
    {
        return WarehouseControllerHelpers.UniqueConflict(
            "An aisle with this code already exists in the selected zone.");
    }
}
