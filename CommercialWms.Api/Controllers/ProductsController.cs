using CommercialWms.Api.Contracts.Common;
using CommercialWms.Api.Contracts.Products;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(CommercialWmsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetProducts(
        [FromQuery] CatalogListQuery query,
        CancellationToken cancellationToken)
    {
        var products = dbContext.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            products = products.Where(product =>
                product.SKU.Contains(search)
                || product.ProductName.Contains(search)
                || (product.Barcode != null && product.Barcode.Contains(search)));
        }

        if (query.IsActive.HasValue)
        {
            products = products.Where(product => product.IsActive == query.IsActive.Value);
        }

        var totalCount = await products.CountAsync(cancellationToken);
        var result = await products
            .OrderBy(product => product.ProductId)
            .ApplyCatalogPaging(query, totalCount, Response.Headers)
            .Select(product => ToResponse(
                product,
                dbContext.ProductUnits
                    .Where(link => link.ProductId == product.ProductId && link.IsBaseUnit)
                    .Select(link => (int?)link.UnitId)
                    .SingleOrDefault()))
            .ToListAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProduct(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .Where(product => product.ProductId == id)
            .Select(product => ToResponse(
                product,
                dbContext.ProductUnits
                    .Where(link => link.ProductId == product.ProductId && link.IsBaseUnit)
                    .Select(link => (int?)link.UnitId)
                    .SingleOrDefault()))
            .SingleOrDefaultAsync(cancellationToken);

        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> CreateProduct(
        [FromBody] ProductCreateRequest request,
        CancellationToken cancellationToken)
    {
        var sku = request.SKU.Trim();
        var barcode = NormalizeOptional(request.Barcode);

        var referenceError = await ValidateReferencesAsync(
            request.CategoryId,
            request.BrandId,
            request.UnitId,
            requireActiveBrand: true,
            requireActiveUnit: true,
            cancellationToken);
        if (referenceError is not null)
        {
            return BadRequest(CreateProblem(StatusCodes.Status400BadRequest, referenceError));
        }

        if (await dbContext.Products.AnyAsync(product => product.SKU == sku, cancellationToken))
        {
            return SkuConflict();
        }

        if (barcode is not null
            && await dbContext.Products.AnyAsync(product => product.Barcode == barcode, cancellationToken))
        {
            return BarcodeConflict();
        }

        var product = new Product
        {
            SKU = sku,
            ProductName = request.ProductName.Trim(),
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            Barcode = barcode,
            Description = request.Description,
            IsPerishable = request.IsPerishable ?? true,
            IsActive = request.IsActive ?? true
        };

        dbContext.Products.Add(product);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.ProductUnits.Add(new ProductUnit
            {
                ProductId = product.ProductId,
                UnitId = request.UnitId,
                ConversionToBase = 1,
                IsBaseUnit = true
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsProductUniqueConstraintViolation(exception, "IX_Products_SKU"))
        {
            return SkuConflict();
        }
        catch (DbUpdateException exception) when (IsProductUniqueConstraintViolation(exception, "IX_Products_Barcode"))
        {
            return BarcodeConflict();
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            return BadRequest(CreateProblem(
                StatusCodes.Status400BadRequest,
                "The specified category, brand, or unit does not exist."));
        }

        var response = ToResponse(product, request.UnitId);
        return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(
        int id,
        [FromBody] ProductUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .SingleOrDefaultAsync(product => product.ProductId == id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        var sku = request.SKU.Trim();
        var barcode = NormalizeOptional(request.Barcode);
        var referenceError = await ValidateReferencesAsync(
            request.CategoryId,
            request.BrandId,
            request.UnitId,
            requireActiveBrand: false,
            requireActiveUnit: false,
            cancellationToken);
        if (referenceError is not null)
        {
            return BadRequest(CreateProblem(StatusCodes.Status400BadRequest, referenceError));
        }

        if (await dbContext.Products.AnyAsync(
                existing => existing.ProductId != id && existing.SKU == sku,
                cancellationToken))
        {
            return SkuConflict();
        }

        if (barcode is not null
            && await dbContext.Products.AnyAsync(
                existing => existing.ProductId != id && existing.Barcode == barcode,
                cancellationToken))
        {
            return BarcodeConflict();
        }

        var currentUnitId = await dbContext.ProductUnits
            .Where(link => link.ProductId == id && link.IsBaseUnit)
            .Select(link => (int?)link.UnitId)
            .SingleOrDefaultAsync(cancellationToken);
        if (!currentUnitId.HasValue)
        {
            return Conflict(CreateProblem(
                StatusCodes.Status409Conflict,
                "The product does not have a base unit. Repair its unit association before updating it."));
        }

        if (currentUnitId.Value != request.UnitId)
        {
            return BadRequest(CreateProblem(
                StatusCodes.Status400BadRequest,
                "The base unit cannot be changed without defining conversion values for existing product units."));
        }

        product.SKU = sku;
        product.ProductName = request.ProductName.Trim();
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.Barcode = barcode;
        product.Description = request.Description;
        product.IsPerishable = request.IsPerishable;
        product.IsActive = request.IsActive;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsProductUniqueConstraintViolation(exception, "IX_Products_SKU"))
        {
            return SkuConflict();
        }
        catch (DbUpdateException exception) when (IsProductUniqueConstraintViolation(exception, "IX_Products_Barcode"))
        {
            return BarcodeConflict();
        }

        return Ok(ToResponse(product, currentUnitId));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .SingleOrDefaultAsync(product => product.ProductId == id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        if (await IsProductReferencedAsync(id, cancellationToken))
        {
            return ProductInUseConflict();
        }

        dbContext.Products.Remove(product);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            return ProductInUseConflict();
        }

        return NoContent();
    }

    private async Task<string?> ValidateReferencesAsync(
        int categoryId,
        int? brandId,
        int unitId,
        bool requireActiveBrand,
        bool requireActiveUnit,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.ProductCategories.AnyAsync(category => category.CategoryId == categoryId, cancellationToken))
        {
            return $"Product category {categoryId} was not found.";
        }

        if (brandId.HasValue)
        {
            var brandIsActive = await dbContext.Brands
                .Where(brand => brand.BrandId == brandId.Value)
                .Select(brand => (bool?)brand.IsActive)
                .SingleOrDefaultAsync(cancellationToken);

            if (!brandIsActive.HasValue)
            {
                return $"Brand {brandId.Value} was not found.";
            }

            if (requireActiveBrand && !brandIsActive.Value)
            {
                return $"Brand {brandId.Value} is inactive and cannot be assigned to a new product.";
            }
        }

        var unitIsActive = await dbContext.Units
            .Where(unit => unit.UnitId == unitId)
            .Select(unit => (bool?)unit.IsActive)
            .SingleOrDefaultAsync(cancellationToken);
        if (!unitIsActive.HasValue)
        {
            return $"Unit {unitId} was not found.";
        }

        if (requireActiveUnit && !unitIsActive.Value)
        {
            return $"Unit {unitId} is inactive and cannot be assigned to a product.";
        }

        return null;
    }

    private async Task<bool> IsProductReferencedAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.CustomerReturnDetails.AnyAsync(item => item.ProductId == id, cancellationToken)
            || await dbContext.GoodsReceiptDetails.AnyAsync(item => item.ProductId == id, cancellationToken)
            || await dbContext.InventoryLots.AnyAsync(item => item.ProductId == id, cancellationToken)
            || await dbContext.ProductStorageRequirements.AnyAsync(item => item.ProductId == id, cancellationToken)
            || await dbContext.ProductUnits.AnyAsync(item => item.ProductId == id, cancellationToken)
            || await dbContext.ProductWarehousePolicies.AnyAsync(item => item.ProductId == id, cancellationToken)
            || await dbContext.PurchaseOrderDetails.AnyAsync(item => item.ProductId == id, cancellationToken)
            || await dbContext.PurchasePriceHistories.AnyAsync(item => item.ProductId == id, cancellationToken)
            || await dbContext.SalesOrderDetails.AnyAsync(item => item.ProductId == id, cancellationToken)
            || await dbContext.StockAdjustmentDetails.AnyAsync(item => item.ProductId == id, cancellationToken)
            || await dbContext.StockTransferDetails.AnyAsync(item => item.ProductId == id, cancellationToken)
            || await dbContext.SupplierProducts.AnyAsync(item => item.ProductId == id, cancellationToken);
    }

    private static ProductResponse ToResponse(Product product, int? unitId)
    {
        return new ProductResponse(
            product.ProductId,
            product.SKU,
            product.ProductName,
            product.CategoryId,
            unitId,
            product.BrandId,
            product.Barcode,
            product.Description,
            product.IsPerishable,
            product.IsActive,
            product.CreatedAt);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsProductUniqueConstraintViolation(DbUpdateException exception, string indexName)
    {
        return exception.GetBaseException() is SqlException { Number: 2601 or 2627 } sqlException
            && sqlException.Message.Contains(indexName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsForeignKeyViolation(DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException { Number: 547 };
    }

    private ConflictObjectResult SkuConflict()
    {
        return Conflict(CreateProblem(StatusCodes.Status409Conflict, "A product with this SKU already exists."));
    }

    private ConflictObjectResult BarcodeConflict()
    {
        return Conflict(CreateProblem(StatusCodes.Status409Conflict, "A product with this barcode already exists."));
    }

    private ConflictObjectResult ProductInUseConflict()
    {
        return Conflict(CreateProblem(
            StatusCodes.Status409Conflict,
            "The product cannot be deleted because it is referenced by other records."));
    }

    private static ProblemDetails CreateProblem(int status, string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = status == StatusCodes.Status409Conflict ? "Conflict" : "Invalid product reference",
            Detail = detail
        };
    }
}
