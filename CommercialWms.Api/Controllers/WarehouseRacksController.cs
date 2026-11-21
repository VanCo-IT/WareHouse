using System.ComponentModel.DataAnnotations;
using CommercialWms.Api.Contracts.Common;
using CommercialWms.Api.Contracts.WarehouseRacks;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WarehouseRacksController(CommercialWmsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseRackResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WarehouseRackResponse>>> GetRacks(
        [FromQuery] CatalogListQuery query,
        [FromQuery, Range(1, int.MaxValue)] int? aisleId,
        CancellationToken cancellationToken)
    {
        if (aisleId.HasValue && !await dbContext.WarehouseAisles.AnyAsync(
                item => item.AisleId == aisleId.Value,
                cancellationToken))
        {
            return WarehouseControllerHelpers.InvalidReference(
                $"Aisle {aisleId.Value} was not found.");
        }

        var racks = dbContext.WarehouseRacks.AsNoTracking();
        if (aisleId.HasValue)
        {
            racks = racks.Where(item => item.AisleId == aisleId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            racks = racks.Where(item => item.RackCode.Contains(search));
        }

        var totalCount = await racks.CountAsync(cancellationToken);
        var result = await racks
            .OrderBy(item => item.RackCode)
            .ApplyCatalogPaging(query, totalCount, Response.Headers)
            .Select(item => ToResponse(item))
            .ToListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WarehouseRackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarehouseRackResponse>> GetRack(int id, CancellationToken cancellationToken)
    {
        var rack = await dbContext.WarehouseRacks
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.RackId == id, cancellationToken);
        return rack is null ? NotFound() : Ok(ToResponse(rack));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WarehouseRackResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WarehouseRackResponse>> CreateRack(
        WarehouseRackCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!WarehouseControllerHelpers.HasRequiredText(request.RackCode))
        {
            return WarehouseControllerHelpers.InvalidInput("Rack code must not be blank.");
        }

        if (!await dbContext.WarehouseAisles.AnyAsync(
                item => item.AisleId == request.AisleId,
                cancellationToken))
        {
            return WarehouseControllerHelpers.InvalidReference(
                $"Aisle {request.AisleId} was not found.");
        }

        var rack = new WarehouseRack
        {
            AisleId = request.AisleId,
            RackCode = request.RackCode.Trim()
        };
        dbContext.WarehouseRacks.Add(rack);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_WarehouseRacks_AisleId_RackCode"))
        {
            return RackCodeConflict();
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseControllerHelpers.InvalidReference("The specified aisle does not exist.");
        }

        return CreatedAtAction(nameof(GetRack), new { id = rack.RackId }, ToResponse(rack));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WarehouseRackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WarehouseRackResponse>> UpdateRack(
        int id,
        WarehouseRackUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var rack = await dbContext.WarehouseRacks
            .SingleOrDefaultAsync(item => item.RackId == id, cancellationToken);
        if (rack is null)
        {
            return NotFound();
        }

        if (!WarehouseControllerHelpers.HasRequiredText(request.RackCode))
        {
            return WarehouseControllerHelpers.InvalidInput("Rack code must not be blank.");
        }

        if (!await dbContext.WarehouseAisles.AnyAsync(
                item => item.AisleId == request.AisleId,
                cancellationToken))
        {
            return WarehouseControllerHelpers.InvalidReference(
                $"Aisle {request.AisleId} was not found.");
        }

        if (rack.AisleId != request.AisleId
            && await dbContext.WarehouseShelves.AnyAsync(item => item.RackId == id, cancellationToken))
        {
            return WarehouseControllerHelpers.InUseConflict(
                "A rack with child shelves cannot be moved to another aisle.");
        }

        rack.AisleId = request.AisleId;
        rack.RackCode = request.RackCode.Trim();
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_WarehouseRacks_AisleId_RackCode"))
        {
            return RackCodeConflict();
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseControllerHelpers.InvalidReference("The specified aisle does not exist.");
        }

        return Ok(ToResponse(rack));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteRack(int id, CancellationToken cancellationToken)
    {
        var rack = await dbContext.WarehouseRacks
            .SingleOrDefaultAsync(item => item.RackId == id, cancellationToken);
        if (rack is null)
        {
            return NotFound();
        }

        if (await dbContext.WarehouseShelves.AnyAsync(item => item.RackId == id, cancellationToken))
        {
            return WarehouseControllerHelpers.InUseConflict(
                "The rack cannot be deleted while it contains shelves.");
        }

        dbContext.WarehouseRacks.Remove(rack);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseControllerHelpers.InUseConflict(
                "The rack is referenced by other records and cannot be deleted.");
        }

        return NoContent();
    }

    private static WarehouseRackResponse ToResponse(WarehouseRack rack)
    {
        return new WarehouseRackResponse(rack.RackId, rack.AisleId, rack.RackCode);
    }

    private ConflictObjectResult RackCodeConflict()
    {
        return WarehouseControllerHelpers.UniqueConflict(
            "A rack with this code already exists in the selected aisle.");
    }
}
