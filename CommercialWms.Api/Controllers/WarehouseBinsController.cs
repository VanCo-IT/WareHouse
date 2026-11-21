using System.ComponentModel.DataAnnotations;
using CommercialWms.Api.Contracts.Common;
using CommercialWms.Api.Contracts.WarehouseBins;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WarehouseBinsController(CommercialWmsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseBinResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WarehouseBinResponse>>> GetBins(
        [FromQuery] CatalogListQuery query,
        [FromQuery, Range(1, int.MaxValue)] int? shelfId,
        CancellationToken cancellationToken)
    {
        if (shelfId.HasValue && !await dbContext.WarehouseShelves.AnyAsync(
                item => item.ShelfId == shelfId.Value,
                cancellationToken))
        {
            return WarehouseControllerHelpers.InvalidReference(
                $"Shelf {shelfId.Value} was not found.");
        }

        var bins = dbContext.WarehouseBins.AsNoTracking();
        if (shelfId.HasValue)
        {
            bins = bins.Where(item => item.ShelfId == shelfId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            bins = bins.Where(item => item.BinCode.Contains(search));
        }

        if (query.IsActive.HasValue)
        {
            bins = bins.Where(item => item.IsActive == query.IsActive.Value);
        }

        var totalCount = await bins.CountAsync(cancellationToken);
        var result = await bins
            .OrderBy(item => item.BinCode)
            .ApplyCatalogPaging(query, totalCount, Response.Headers)
            .Select(item => ToResponse(item))
            .ToListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WarehouseBinResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarehouseBinResponse>> GetBin(int id, CancellationToken cancellationToken)
    {
        var bin = await dbContext.WarehouseBins
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.BinId == id, cancellationToken);
        return bin is null ? NotFound() : Ok(ToResponse(bin));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WarehouseBinResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WarehouseBinResponse>> CreateBin(
        WarehouseBinCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!WarehouseControllerHelpers.HasRequiredText(request.BinCode))
        {
            return WarehouseControllerHelpers.InvalidInput("Bin code must not be blank.");
        }

        if (request.Capacity.HasValue && decimal.Round(request.Capacity.Value, 3) != request.Capacity.Value)
        {
            return WarehouseControllerHelpers.InvalidInput(
                "Bin capacity supports no more than three decimal places.");
        }

        if (!await dbContext.WarehouseShelves.AnyAsync(
                item => item.ShelfId == request.ShelfId,
                cancellationToken))
        {
            return WarehouseControllerHelpers.InvalidReference(
                $"Shelf {request.ShelfId} was not found.");
        }

        var bin = new WarehouseBin
        {
            ShelfId = request.ShelfId,
            BinCode = request.BinCode.Trim(),
            Capacity = request.Capacity,
            IsActive = request.IsActive ?? true
        };
        dbContext.WarehouseBins.Add(bin);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_WarehouseBins_ShelfId_BinCode"))
        {
            return BinCodeConflict();
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseControllerHelpers.InvalidReference("The specified shelf does not exist.");
        }

        return CreatedAtAction(nameof(GetBin), new { id = bin.BinId }, ToResponse(bin));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WarehouseBinResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WarehouseBinResponse>> UpdateBin(
        int id,
        WarehouseBinUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var bin = await dbContext.WarehouseBins
            .SingleOrDefaultAsync(item => item.BinId == id, cancellationToken);
        if (bin is null)
        {
            return NotFound();
        }

        if (!WarehouseControllerHelpers.HasRequiredText(request.BinCode))
        {
            return WarehouseControllerHelpers.InvalidInput("Bin code must not be blank.");
        }

        if (request.Capacity.HasValue && decimal.Round(request.Capacity.Value, 3) != request.Capacity.Value)
        {
            return WarehouseControllerHelpers.InvalidInput(
                "Bin capacity supports no more than three decimal places.");
        }

        if (!await dbContext.WarehouseShelves.AnyAsync(
                item => item.ShelfId == request.ShelfId,
                cancellationToken))
        {
            return WarehouseControllerHelpers.InvalidReference(
                $"Shelf {request.ShelfId} was not found.");
        }

        if (bin.ShelfId != request.ShelfId
            && await IsBinReferencedAsync(id, cancellationToken))
        {
            return WarehouseControllerHelpers.InUseConflict(
                "A bin with stock or operational history cannot be moved to another shelf.");
        }

        bin.ShelfId = request.ShelfId;
        bin.BinCode = request.BinCode.Trim();
        bin.Capacity = request.Capacity;
        bin.IsActive = request.IsActive;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_WarehouseBins_ShelfId_BinCode"))
        {
            return BinCodeConflict();
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseControllerHelpers.InvalidReference("The specified shelf does not exist.");
        }

        return Ok(ToResponse(bin));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteBin(int id, CancellationToken cancellationToken)
    {
        var bin = await dbContext.WarehouseBins
            .SingleOrDefaultAsync(item => item.BinId == id, cancellationToken);
        if (bin is null)
        {
            return NotFound();
        }

        if (await IsBinReferencedAsync(id, cancellationToken))
        {
            return WarehouseControllerHelpers.InUseConflict(
                "The bin cannot be deleted because it contains stock or is referenced by operational records.");
        }

        dbContext.WarehouseBins.Remove(bin);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseControllerHelpers.InUseConflict(
                "The bin is referenced by another record and cannot be deleted.");
        }

        return NoContent();
    }

    private async Task<bool> IsBinReferencedAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.InventoryBalances.AnyAsync(item => item.BinId == id, cancellationToken)
            || await dbContext.InventoryTransactions.AnyAsync(item => item.BinId == id, cancellationToken)
            || await dbContext.StockReservations.AnyAsync(item => item.BinId == id, cancellationToken)
            || await dbContext.StockCountDetails.AnyAsync(item => item.BinId == id, cancellationToken)
            || await dbContext.StockAdjustmentDetails.AnyAsync(item => item.BinId == id, cancellationToken)
            || await dbContext.StockTransferDetails.AnyAsync(
                item => item.SourceBinId == id || item.DestinationBinId == id,
                cancellationToken);
    }

    private static WarehouseBinResponse ToResponse(WarehouseBin bin)
    {
        return new WarehouseBinResponse(
            bin.BinId,
            bin.ShelfId,
            bin.BinCode,
            bin.Capacity,
            bin.IsActive);
    }

    private ConflictObjectResult BinCodeConflict()
    {
        return WarehouseControllerHelpers.UniqueConflict(
            "A bin with this code already exists in the selected shelf.");
    }
}
