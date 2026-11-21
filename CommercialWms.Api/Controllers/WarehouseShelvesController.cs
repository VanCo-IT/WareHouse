using System.ComponentModel.DataAnnotations;
using CommercialWms.Api.Contracts.Common;
using CommercialWms.Api.Contracts.WarehouseShelves;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WarehouseShelvesController(CommercialWmsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseShelfResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WarehouseShelfResponse>>> GetShelves(
        [FromQuery] CatalogListQuery query,
        [FromQuery, Range(1, int.MaxValue)] int? rackId,
        CancellationToken cancellationToken)
    {
        if (rackId.HasValue && !await dbContext.WarehouseRacks.AnyAsync(
                item => item.RackId == rackId.Value,
                cancellationToken))
        {
            return WarehouseControllerHelpers.InvalidReference($"Rack {rackId.Value} was not found.");
        }

        var shelves = dbContext.WarehouseShelves.AsNoTracking();
        if (rackId.HasValue)
        {
            shelves = shelves.Where(item => item.RackId == rackId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            shelves = shelves.Where(item => item.ShelfCode.Contains(search));
        }

        var totalCount = await shelves.CountAsync(cancellationToken);
        var result = await shelves
            .OrderBy(item => item.ShelfCode)
            .ApplyCatalogPaging(query, totalCount, Response.Headers)
            .Select(item => ToResponse(item))
            .ToListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WarehouseShelfResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarehouseShelfResponse>> GetShelf(int id, CancellationToken cancellationToken)
    {
        var shelf = await dbContext.WarehouseShelves
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.ShelfId == id, cancellationToken);
        return shelf is null ? NotFound() : Ok(ToResponse(shelf));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WarehouseShelfResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WarehouseShelfResponse>> CreateShelf(
        WarehouseShelfCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!WarehouseControllerHelpers.HasRequiredText(request.ShelfCode))
        {
            return WarehouseControllerHelpers.InvalidInput("Shelf code must not be blank.");
        }

        if (!await dbContext.WarehouseRacks.AnyAsync(
                item => item.RackId == request.RackId,
                cancellationToken))
        {
            return WarehouseControllerHelpers.InvalidReference($"Rack {request.RackId} was not found.");
        }

        var shelf = new WarehouseShelf
        {
            RackId = request.RackId,
            ShelfCode = request.ShelfCode.Trim()
        };
        dbContext.WarehouseShelves.Add(shelf);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_WarehouseShelves_RackId_ShelfCode"))
        {
            return ShelfCodeConflict();
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseControllerHelpers.InvalidReference("The specified rack does not exist.");
        }

        return CreatedAtAction(nameof(GetShelf), new { id = shelf.ShelfId }, ToResponse(shelf));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WarehouseShelfResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WarehouseShelfResponse>> UpdateShelf(
        int id,
        WarehouseShelfUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var shelf = await dbContext.WarehouseShelves
            .SingleOrDefaultAsync(item => item.ShelfId == id, cancellationToken);
        if (shelf is null)
        {
            return NotFound();
        }

        if (!WarehouseControllerHelpers.HasRequiredText(request.ShelfCode))
        {
            return WarehouseControllerHelpers.InvalidInput("Shelf code must not be blank.");
        }

        if (!await dbContext.WarehouseRacks.AnyAsync(
                item => item.RackId == request.RackId,
                cancellationToken))
        {
            return WarehouseControllerHelpers.InvalidReference($"Rack {request.RackId} was not found.");
        }

        if (shelf.RackId != request.RackId
            && await dbContext.WarehouseBins.AnyAsync(item => item.ShelfId == id, cancellationToken))
        {
            return WarehouseControllerHelpers.InUseConflict(
                "A shelf with child bins cannot be moved to another rack.");
        }

        shelf.RackId = request.RackId;
        shelf.ShelfCode = request.ShelfCode.Trim();
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_WarehouseShelves_RackId_ShelfCode"))
        {
            return ShelfCodeConflict();
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseControllerHelpers.InvalidReference("The specified rack does not exist.");
        }

        return Ok(ToResponse(shelf));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteShelf(int id, CancellationToken cancellationToken)
    {
        var shelf = await dbContext.WarehouseShelves
            .SingleOrDefaultAsync(item => item.ShelfId == id, cancellationToken);
        if (shelf is null)
        {
            return NotFound();
        }

        if (await dbContext.WarehouseBins.AnyAsync(item => item.ShelfId == id, cancellationToken))
        {
            return WarehouseControllerHelpers.InUseConflict(
                "The shelf cannot be deleted while it contains bins.");
        }

        dbContext.WarehouseShelves.Remove(shelf);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseControllerHelpers.InUseConflict(
                "The shelf is referenced by other records and cannot be deleted.");
        }

        return NoContent();
    }

    private static WarehouseShelfResponse ToResponse(WarehouseShelf shelf)
    {
        return new WarehouseShelfResponse(shelf.ShelfId, shelf.RackId, shelf.ShelfCode);
    }

    private ConflictObjectResult ShelfCodeConflict()
    {
        return WarehouseControllerHelpers.UniqueConflict(
            "A shelf with this code already exists in the selected rack.");
    }
}
