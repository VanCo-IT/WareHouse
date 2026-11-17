using System.ComponentModel.DataAnnotations;
using CommercialWms.Api.Contracts.Suppliers;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SuppliersController(CommercialWmsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SupplierResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SupplierResponse>>> GetSuppliers(CancellationToken cancellationToken)
    {
        var suppliers = await dbContext.Suppliers
            .AsNoTracking()
            .OrderBy(supplier => supplier.SupplierId)
            .Select(supplier => ToResponse(supplier))
            .ToListAsync(cancellationToken);

        return Ok(suppliers);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierResponse>> GetSupplier(int id, CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers
            .AsNoTracking()
            .Where(item => item.SupplierId == id)
            .Select(item => ToResponse(item))
            .SingleOrDefaultAsync(cancellationToken);

        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SupplierResponse>> CreateSupplier(
        SupplierCreateRequest request,
        CancellationToken cancellationToken)
    {
        var code = request.SupplierCode.Trim();
        if (!await dbContext.Countries.AnyAsync(country => country.CountryId == request.CountryId, cancellationToken))
        {
            return BadRequest(CreateProblem(StatusCodes.Status400BadRequest, $"Country {request.CountryId} was not found."));
        }

        if (await dbContext.Suppliers.AnyAsync(supplier => supplier.SupplierCode == code, cancellationToken))
        {
            return SupplierCodeConflict();
        }

        var supplier = new Supplier
        {
            SupplierCode = code,
            SupplierName = request.SupplierName.Trim(),
            CountryId = request.CountryId,
            TaxCode = NormalizeOptional(request.TaxCode),
            Email = NormalizeOptional(request.Email),
            Phone = NormalizeOptional(request.Phone),
            Address = NormalizeOptional(request.Address),
            IsActive = request.IsActive ?? true
        };

        dbContext.Suppliers.Add(supplier);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception, "IX_Suppliers_SupplierCode"))
        {
            return SupplierCodeConflict();
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            return BadRequest(CreateProblem(StatusCodes.Status400BadRequest, "The specified country does not exist."));
        }

        var response = ToResponse(supplier);
        return CreatedAtAction(nameof(GetSupplier), new { id = supplier.SupplierId }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SupplierResponse>> UpdateSupplier(
        int id,
        SupplierUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers
            .SingleOrDefaultAsync(item => item.SupplierId == id, cancellationToken);
        if (supplier is null)
        {
            return NotFound();
        }

        if (!await dbContext.Countries.AnyAsync(country => country.CountryId == request.CountryId, cancellationToken))
        {
            return BadRequest(CreateProblem(StatusCodes.Status400BadRequest, $"Country {request.CountryId} was not found."));
        }

        var code = request.SupplierCode.Trim();
        if (await dbContext.Suppliers.AnyAsync(
                existing => existing.SupplierId != id && existing.SupplierCode == code,
                cancellationToken))
        {
            return SupplierCodeConflict();
        }

        supplier.SupplierCode = code;
        supplier.SupplierName = request.SupplierName.Trim();
        supplier.CountryId = request.CountryId;
        supplier.TaxCode = NormalizeOptional(request.TaxCode);
        supplier.Email = NormalizeOptional(request.Email);
        supplier.Phone = NormalizeOptional(request.Phone);
        supplier.Address = NormalizeOptional(request.Address);
        supplier.IsActive = request.IsActive!.Value;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception, "IX_Suppliers_SupplierCode"))
        {
            return SupplierCodeConflict();
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            return BadRequest(CreateProblem(StatusCodes.Status400BadRequest, "The specified country does not exist."));
        }

        return Ok(ToResponse(supplier));
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierResponse>> UpdateSupplierStatus(
        int id,
        SupplierStatusUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Suppliers
            .SingleOrDefaultAsync(item => item.SupplierId == id, cancellationToken);
        if (supplier is null)
        {
            return NotFound();
        }

        supplier.IsActive = request.IsActive!.Value;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(supplier));
    }

    [HttpGet("{id:int}/products")]
    [ProducesResponseType(typeof(IReadOnlyList<SupplierProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SupplierProductResponse>>> GetSupplierProducts(
        int id,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Suppliers.AnyAsync(supplier => supplier.SupplierId == id, cancellationToken))
        {
            return NotFound();
        }

        var products = await SupplierProductQueryWithDetails()
            .Where(link => link.SupplierId == id)
            .OrderBy(link => link.ProductId)
            .ThenBy(link => link.ManufacturerId)
            .Select(link => ToResponse(link))
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    [HttpPost("{id:int}/products")]
    [ProducesResponseType(typeof(SupplierProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SupplierProductResponse>> AddSupplierProduct(
        int id,
        SupplierProductCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Suppliers.AnyAsync(supplier => supplier.SupplierId == id, cancellationToken))
        {
            return NotFound();
        }

        if (!await dbContext.Products.AnyAsync(product => product.ProductId == request.ProductId, cancellationToken))
        {
            return NotFound(CreateProblem(StatusCodes.Status404NotFound, $"Product {request.ProductId} was not found."));
        }

        if (!await dbContext.Manufacturers.AnyAsync(
                manufacturer => manufacturer.ManufacturerId == request.ManufacturerId,
                cancellationToken))
        {
            return NotFound(CreateProblem(
                StatusCodes.Status404NotFound,
                $"Manufacturer {request.ManufacturerId} was not found."));
        }

        if (await dbContext.SupplierProducts.AnyAsync(
                link => link.SupplierId == id
                    && link.ProductId == request.ProductId
                    && link.ManufacturerId == request.ManufacturerId,
                cancellationToken))
        {
            return SupplierProductConflict();
        }

        var link = new SupplierProduct
        {
            SupplierId = id,
            ProductId = request.ProductId,
            ManufacturerId = request.ManufacturerId,
            SupplierProductCode = NormalizeOptional(request.SupplierProductCode),
            LeadTimeDays = request.LeadTimeDays,
            IsActive = request.IsActive ?? true
        };

        dbContext.SupplierProducts.Add(link);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            IsUniqueViolation(exception, "IX_SupplierProducts_SupplierId_ProductId_ManufacturerId"))
        {
            return SupplierProductConflict();
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            return BadRequest(CreateProblem(StatusCodes.Status400BadRequest, "A supplier, product, or manufacturer reference is invalid."));
        }

        var response = await SupplierProductQueryWithDetails()
            .Where(item => item.SupplierProductId == link.SupplierProductId)
            .Select(item => ToResponse(item))
            .SingleAsync(cancellationToken);

        return CreatedAtAction(nameof(GetSupplierProducts), new { id }, response);
    }

    [HttpPut("{id:int}/products/{productId:int}")]
    [ProducesResponseType(typeof(SupplierProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateSupplierProduct(
        int id,
        int productId,
        [FromQuery, Range(1, int.MaxValue)] int? manufacturerId,
        SupplierProductUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var linkResult = await FindSupplierProductAsync(id, productId, manufacturerId, cancellationToken);
        if (linkResult.Error is not null)
        {
            return linkResult.Error;
        }

        var link = linkResult.Link!;
        link.SupplierProductCode = NormalizeOptional(request.SupplierProductCode);
        link.LeadTimeDays = request.LeadTimeDays;
        link.IsActive = request.IsActive!.Value;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = await SupplierProductQueryWithDetails()
            .Where(item => item.SupplierProductId == link.SupplierProductId)
            .Select(item => ToResponse(item))
            .SingleAsync(cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{id:int}/products/{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveSupplierProduct(
        int id,
        int productId,
        [FromQuery, Range(1, int.MaxValue)] int? manufacturerId,
        CancellationToken cancellationToken)
    {
        var linkResult = await FindSupplierProductAsync(id, productId, manufacturerId, cancellationToken);
        if (linkResult.Error is not null)
        {
            return linkResult.Error;
        }

        var link = linkResult.Link!;
        if (await HasPurchaseHistoryAsync(id, productId, cancellationToken))
        {
            return Conflict(CreateProblem(
                StatusCodes.Status409Conflict,
                "The supplier-product link cannot be removed because purchase history exists for this supplier and product."));
        }

        dbContext.SupplierProducts.Remove(link);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            return Conflict(CreateProblem(
                StatusCodes.Status409Conflict,
                "The supplier-product link is referenced by other records and cannot be removed."));
        }

        return NoContent();
    }

    private IQueryable<SupplierProductResponseProjection> SupplierProductQueryWithDetails()
    {
        return from link in dbContext.SupplierProducts.AsNoTracking()
               join product in dbContext.Products.AsNoTracking() on link.ProductId equals product.ProductId
               join manufacturer in dbContext.Manufacturers.AsNoTracking()
                   on link.ManufacturerId equals manufacturer.ManufacturerId
               select new SupplierProductResponseProjection
               {
                   SupplierProductId = link.SupplierProductId,
                   SupplierId = link.SupplierId,
                   ProductId = link.ProductId,
                   ProductSKU = product.SKU,
                   ProductName = product.ProductName,
                   ManufacturerId = link.ManufacturerId,
                   ManufacturerName = manufacturer.ManufacturerName,
                   SupplierProductCode = link.SupplierProductCode,
                   LeadTimeDays = link.LeadTimeDays,
                   IsActive = link.IsActive
               };
    }

    private async Task<(SupplierProduct? Link, IActionResult? Error)> FindSupplierProductAsync(
        int supplierId,
        int productId,
        int? manufacturerId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Suppliers.AnyAsync(supplier => supplier.SupplierId == supplierId, cancellationToken))
        {
            return (null, NotFound());
        }

        var query = dbContext.SupplierProducts
            .Where(link => link.SupplierId == supplierId && link.ProductId == productId);
        if (manufacturerId.HasValue)
        {
            query = query.Where(link => link.ManufacturerId == manufacturerId.Value);
        }

        var links = await query.Take(2).ToListAsync(cancellationToken);
        if (links.Count == 0)
        {
            return (null, NotFound());
        }

        if (links.Count > 1)
        {
            return (null, Conflict(CreateProblem(
                StatusCodes.Status409Conflict,
                "More than one manufacturer is linked to this product; specify manufacturerId in the query string.")));
        }

        return (links[0], null);
    }

    private async Task<bool> HasPurchaseHistoryAsync(
        int supplierId,
        int productId,
        CancellationToken cancellationToken)
    {
        return await dbContext.PurchaseOrders.AnyAsync(
                order => order.SupplierId == supplierId
                    && dbContext.PurchaseOrderDetails.Any(
                        detail => detail.PurchaseOrderId == order.PurchaseOrderId && detail.ProductId == productId),
                cancellationToken)
            || await dbContext.PurchasePriceHistories.AnyAsync(
                history => history.SupplierId == supplierId && history.ProductId == productId,
                cancellationToken);
    }

    private static SupplierProductResponse ToResponse(SupplierProductResponseProjection projection)
    {
        return new SupplierProductResponse(
            projection.SupplierProductId,
            projection.SupplierId,
            projection.ProductId,
            projection.ProductSKU,
            projection.ProductName,
            projection.ManufacturerId,
            projection.ManufacturerName,
            projection.SupplierProductCode,
            projection.LeadTimeDays,
            projection.IsActive);
    }

    private static SupplierResponse ToResponse(Supplier supplier)
    {
        return new SupplierResponse(
            supplier.SupplierId,
            supplier.SupplierCode,
            supplier.SupplierName,
            supplier.CountryId,
            supplier.TaxCode,
            supplier.Email,
            supplier.Phone,
            supplier.Address,
            supplier.IsActive);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsUniqueViolation(DbUpdateException exception, string indexName)
    {
        return exception.GetBaseException() is SqlException { Number: 2601 or 2627 } sqlException
            && sqlException.Message.Contains(indexName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsForeignKeyViolation(DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException { Number: 547 };
    }

    private ConflictObjectResult SupplierCodeConflict()
    {
        return Conflict(CreateProblem(StatusCodes.Status409Conflict, "A supplier with this code already exists."));
    }

    private ConflictObjectResult SupplierProductConflict()
    {
        return Conflict(CreateProblem(
            StatusCodes.Status409Conflict,
            "This supplier, product, and manufacturer combination already exists."));
    }

    private static ProblemDetails CreateProblem(int status, string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = status == StatusCodes.Status409Conflict ? "Conflict" : "Invalid supplier request",
            Detail = detail
        };
    }

    private sealed class SupplierProductResponseProjection
    {
        public long SupplierProductId { get; init; }
        public int SupplierId { get; init; }
        public int ProductId { get; init; }
        public string ProductSKU { get; init; } = string.Empty;
        public string ProductName { get; init; } = string.Empty;
        public int ManufacturerId { get; init; }
        public string ManufacturerName { get; init; } = string.Empty;
        public string? SupplierProductCode { get; init; }
        public int LeadTimeDays { get; init; }
        public bool IsActive { get; init; }
    }
}
