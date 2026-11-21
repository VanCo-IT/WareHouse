using System.ComponentModel.DataAnnotations;
using CommercialWms.Api.Contracts.Common;
using CommercialWms.Api.Contracts.WarehouseZones;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WarehouseZonesController(CommercialWmsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseZoneResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WarehouseZoneResponse>>> GetZones(
        [FromQuery] CatalogListQuery query,
        [FromQuery, Range(1, int.MaxValue)] int? warehouseId,
        CancellationToken cancellationToken)
    {
        if (warehouseId.HasValue && !await dbContext.Warehouses.AnyAsync(
                item => item.WarehouseId == warehouseId.Value,
                cancellationToken))
        {
            return WarehouseControllerHelpers.InvalidReference(
                $"Warehouse {warehouseId.Value} was not found.");
        }

        var zones = dbContext.WarehouseZones.AsNoTracking();
        if (warehouseId.HasValue)
        {
            zones = zones.Where(item => item.WarehouseId == warehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            zones = zones.Where(item => item.ZoneCode.Contains(search) || item.ZoneName.Contains(search));
        }

        var totalCount = await zones.CountAsync(cancellationToken);
        var result = await zones
            .OrderBy(item => item.ZoneCode)
            .ApplyCatalogPaging(query, totalCount, Response.Headers)
            .Select(item => ToResponse(item))
            .ToListAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WarehouseZoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarehouseZoneResponse>> GetZone(int id, CancellationToken cancellationToken)
    {
        var zone = await dbContext.WarehouseZones
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.ZoneId == id, cancellationToken);

        return zone is null ? NotFound() : Ok(ToResponse(zone));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WarehouseZoneResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WarehouseZoneResponse>> CreateZone(
        WarehouseZoneCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!WarehouseControllerHelpers.HasRequiredText(request.ZoneCode, request.ZoneName))
        {
            return WarehouseControllerHelpers.InvalidInput("Zone code and name must not be blank.");
        }

        if (!await dbContext.Warehouses.AnyAsync(
                item => item.WarehouseId == request.WarehouseId,
                cancellationToken))
        {
            return WarehouseControllerHelpers.InvalidReference(
                $"Warehouse {request.WarehouseId} was not found.");
        }

        var zone = new WarehouseZone
        {
            WarehouseId = request.WarehouseId,
            ZoneCode = request.ZoneCode.Trim(),
            ZoneName = request.ZoneName.Trim()
        };
        dbContext.WarehouseZones.Add(zone);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(
                exception,
                "IX_WarehouseZones_WarehouseId_ZoneCode"))
        {
            return ZoneCodeConflict();
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseControllerHelpers.InvalidReference(
                "The specified warehouse does not exist.");
        }

        return CreatedAtAction(nameof(GetZone), new { id = zone.ZoneId }, ToResponse(zone));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WarehouseZoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WarehouseZoneResponse>> UpdateZone(
        int id,
        WarehouseZoneUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var zone = await dbContext.WarehouseZones
            .SingleOrDefaultAsync(item => item.ZoneId == id, cancellationToken);
        if (zone is null)
        {
            return NotFound();
        }

        if (!WarehouseControllerHelpers.HasRequiredText(request.ZoneCode, request.ZoneName))
        {
            return WarehouseControllerHelpers.InvalidInput("Zone code and name must not be blank.");
        }

        if (!await dbContext.Warehouses.AnyAsync(
                item => item.WarehouseId == request.WarehouseId,
                cancellationToken))
        {
            return WarehouseControllerHelpers.InvalidReference(
                $"Warehouse {request.WarehouseId} was not found.");
        }

        if (zone.WarehouseId != request.WarehouseId
            && await dbContext.WarehouseAisles.AnyAsync(item => item.ZoneId == id, cancellationToken))
        {
            return WarehouseControllerHelpers.InUseConflict(
                "A zone with child aisles cannot be moved to another warehouse.");
        }

        zone.WarehouseId = request.WarehouseId;
        zone.ZoneCode = request.ZoneCode.Trim();
        zone.ZoneName = request.ZoneName.Trim();
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(
                exception,
                "IX_WarehouseZones_WarehouseId_ZoneCode"))
        {
            return ZoneCodeConflict();
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseControllerHelpers.InvalidReference(
                "The specified warehouse does not exist.");
        }

        return Ok(ToResponse(zone));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteZone(int id, CancellationToken cancellationToken)
    {
        var zone = await dbContext.WarehouseZones
            .SingleOrDefaultAsync(item => item.ZoneId == id, cancellationToken);
        if (zone is null)
        {
            return NotFound();
        }

        if (await dbContext.WarehouseAisles.AnyAsync(item => item.ZoneId == id, cancellationToken))
        {
            return WarehouseControllerHelpers.InUseConflict(
                "The zone cannot be deleted while it contains aisles.");
        }

        dbContext.WarehouseZones.Remove(zone);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseControllerHelpers.InUseConflict(
                "The zone is referenced by other records and cannot be deleted.");
        }

        return NoContent();
    }

    private static WarehouseZoneResponse ToResponse(WarehouseZone zone)
    {
        return new WarehouseZoneResponse(zone.ZoneId, zone.WarehouseId, zone.ZoneCode, zone.ZoneName);
    }

    private ConflictObjectResult ZoneCodeConflict()
    {
        return WarehouseControllerHelpers.UniqueConflict(
            "A zone with this code already exists in the selected warehouse.");
    }
}
