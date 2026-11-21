using CommercialWms.Api.Contracts.Common;
using CommercialWms.Api.Contracts.WarehouseBins;
using CommercialWms.Api.Contracts.Warehouses;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WarehousesController(CommercialWmsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WarehouseResponse>>> GetWarehouses(
        [FromQuery] CatalogListQuery query,
        CancellationToken cancellationToken)
    {
        var warehouses = dbContext.Warehouses.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            warehouses = warehouses.Where(warehouse =>
                warehouse.WarehouseCode.Contains(search)
                || warehouse.WarehouseName.Contains(search)
                || warehouse.Address.Contains(search));
        }

        if (query.IsActive.HasValue)
        {
            warehouses = warehouses.Where(warehouse => warehouse.IsActive == query.IsActive.Value);
        }

        var totalCount = await warehouses.CountAsync(cancellationToken);
        var result = await warehouses
            .OrderBy(warehouse => warehouse.WarehouseCode)
            .ApplyCatalogPaging(query, totalCount, Response.Headers)
            .Select(warehouse => ToResponse(warehouse))
            .ToListAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WarehouseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarehouseResponse>> GetWarehouse(
        int id,
        CancellationToken cancellationToken)
    {
        var warehouse = await dbContext.Warehouses
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.WarehouseId == id, cancellationToken);

        return warehouse is null ? NotFound() : Ok(ToResponse(warehouse));
    }

    [HttpGet("{id:int}/structure")]
    [ProducesResponseType(typeof(WarehouseStructureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarehouseStructureResponse>> GetStructure(
        int id,
        CancellationToken cancellationToken)
    {
        var warehouse = await dbContext.Warehouses
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.WarehouseId == id, cancellationToken);
        if (warehouse is null)
        {
            return NotFound();
        }

        var zones = await dbContext.WarehouseZones
            .AsNoTracking()
            .Where(item => item.WarehouseId == id)
            .OrderBy(item => item.ZoneCode)
            .ToListAsync(cancellationToken);
        var zoneIds = zones.Select(item => item.ZoneId).ToArray();
        var aisles = await dbContext.WarehouseAisles
            .AsNoTracking()
            .Where(item => zoneIds.Contains(item.ZoneId))
            .OrderBy(item => item.AisleCode)
            .ToListAsync(cancellationToken);
        var aisleIds = aisles.Select(item => item.AisleId).ToArray();
        var racks = await dbContext.WarehouseRacks
            .AsNoTracking()
            .Where(item => aisleIds.Contains(item.AisleId))
            .OrderBy(item => item.RackCode)
            .ToListAsync(cancellationToken);
        var rackIds = racks.Select(item => item.RackId).ToArray();
        var shelves = await dbContext.WarehouseShelves
            .AsNoTracking()
            .Where(item => rackIds.Contains(item.RackId))
            .OrderBy(item => item.ShelfCode)
            .ToListAsync(cancellationToken);
        var shelfIds = shelves.Select(item => item.ShelfId).ToArray();
        var bins = await dbContext.WarehouseBins
            .AsNoTracking()
            .Where(item => shelfIds.Contains(item.ShelfId))
            .OrderBy(item => item.BinCode)
            .ToListAsync(cancellationToken);

        var binsByShelf = bins
            .GroupBy(item => item.ShelfId)
            .ToDictionary(group => group.Key, group => group.Select(ToBinResponse).ToList());
        var shelvesByRack = shelves
            .GroupBy(item => item.RackId)
            .ToDictionary(group => group.Key, group => group.ToList());
        var racksByAisle = racks
            .GroupBy(item => item.AisleId)
            .ToDictionary(group => group.Key, group => group.ToList());
        var aislesByZone = aisles
            .GroupBy(item => item.ZoneId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var structure = zones.Select(zone => new WarehouseZoneNodeResponse(
            zone.ZoneId,
            zone.ZoneCode,
            zone.ZoneName,
            aislesByZone.GetValueOrDefault(zone.ZoneId, [])
                .Select(aisle => new WarehouseAisleNodeResponse(
                    aisle.AisleId,
                    aisle.AisleCode,
                    racksByAisle.GetValueOrDefault(aisle.AisleId, [])
                        .Select(rack => new WarehouseRackNodeResponse(
                            rack.RackId,
                            rack.RackCode,
                            shelvesByRack.GetValueOrDefault(rack.RackId, [])
                                .Select(shelf => new WarehouseShelfNodeResponse(
                                    shelf.ShelfId,
                                    shelf.ShelfCode,
                                    binsByShelf.GetValueOrDefault(shelf.ShelfId, [])))
                                .ToList()))
                        .ToList()))
                .ToList()))
            .ToList();

        return Ok(new WarehouseStructureResponse(ToResponse(warehouse), structure));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WarehouseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WarehouseResponse>> CreateWarehouse(
        WarehouseCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!WarehouseControllerHelpers.HasRequiredText(
                request.WarehouseCode,
                request.WarehouseName,
                request.Address))
        {
            return WarehouseControllerHelpers.InvalidInput(
                "Warehouse code, name, and address must not be blank.");
        }

        var warehouse = new Warehouse
        {
            WarehouseCode = request.WarehouseCode.Trim(),
            WarehouseName = request.WarehouseName.Trim(),
            Address = request.Address.Trim(),
            IsActive = request.IsActive ?? true
        };

        dbContext.Warehouses.Add(warehouse);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_Warehouses_WarehouseCode"))
        {
            return WarehouseControllerHelpers.UniqueConflict(
                "A warehouse with this code already exists.");
        }

        return CreatedAtAction(
            nameof(GetWarehouse),
            new { id = warehouse.WarehouseId },
            ToResponse(warehouse));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WarehouseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WarehouseResponse>> UpdateWarehouse(
        int id,
        WarehouseUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var warehouse = await dbContext.Warehouses
            .SingleOrDefaultAsync(item => item.WarehouseId == id, cancellationToken);
        if (warehouse is null)
        {
            return NotFound();
        }

        if (!WarehouseControllerHelpers.HasRequiredText(
                request.WarehouseCode,
                request.WarehouseName,
                request.Address))
        {
            return WarehouseControllerHelpers.InvalidInput(
                "Warehouse code, name, and address must not be blank.");
        }

        warehouse.WarehouseCode = request.WarehouseCode.Trim();
        warehouse.WarehouseName = request.WarehouseName.Trim();
        warehouse.Address = request.Address.Trim();
        warehouse.IsActive = request.IsActive;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_Warehouses_WarehouseCode"))
        {
            return WarehouseControllerHelpers.UniqueConflict(
                "A warehouse with this code already exists.");
        }

        return Ok(ToResponse(warehouse));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteWarehouse(int id, CancellationToken cancellationToken)
    {
        var warehouse = await dbContext.Warehouses
            .SingleOrDefaultAsync(item => item.WarehouseId == id, cancellationToken);
        if (warehouse is null)
        {
            return NotFound();
        }

        if (await IsReferencedAsync(id, cancellationToken))
        {
            return WarehouseInUseConflict();
        }

        dbContext.Warehouses.Remove(warehouse);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return WarehouseInUseConflict();
        }

        return NoContent();
    }

    private async Task<bool> IsReferencedAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.WarehouseZones.AnyAsync(item => item.WarehouseId == id, cancellationToken)
            || await dbContext.GoodsReceipts.AnyAsync(item => item.WarehouseId == id, cancellationToken)
            || await dbContext.Deliveries.AnyAsync(item => item.WarehouseId == id, cancellationToken)
            || await dbContext.InventoryTransactions.AnyAsync(item => item.WarehouseId == id, cancellationToken)
            || await dbContext.StockAdjustments.AnyAsync(item => item.WarehouseId == id, cancellationToken)
            || await dbContext.ProductWarehousePolicies.AnyAsync(item => item.WarehouseId == id, cancellationToken)
            || await dbContext.StockTransfers.AnyAsync(
                item => item.SourceWarehouseId == id || item.DestinationWarehouseId == id,
                cancellationToken)
            || await dbContext.StockCounts.AnyAsync(item => item.WarehouseId == id, cancellationToken)
            || await dbContext.WarehouseEmployees.AnyAsync(item => item.WarehouseId == id, cancellationToken)
            || await dbContext.WarehouseTemperatureLogs.AnyAsync(item => item.WarehouseId == id, cancellationToken);
    }

    private static WarehouseResponse ToResponse(Warehouse warehouse)
    {
        return new WarehouseResponse(
            warehouse.WarehouseId,
            warehouse.WarehouseCode,
            warehouse.WarehouseName,
            warehouse.Address,
            warehouse.IsActive);
    }

    private static WarehouseBinResponse ToBinResponse(WarehouseBin bin)
    {
        return new WarehouseBinResponse(
            bin.BinId,
            bin.ShelfId,
            bin.BinCode,
            bin.Capacity,
            bin.IsActive);
    }

    private ConflictObjectResult WarehouseInUseConflict()
    {
        return WarehouseControllerHelpers.InUseConflict(
            "The warehouse cannot be deleted while it has storage locations or operational records.");
    }
}
