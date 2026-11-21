using CommercialWms.Api.Contracts.Common;
using CommercialWms.Api.Contracts.Currencies;
using CommercialWms.Api.Data;
using CommercialWms.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CurrenciesController(CommercialWmsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CurrencyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CurrencyResponse>>> GetCurrencies(
        [FromQuery] CatalogListQuery query,
        CancellationToken cancellationToken)
    {
        var currencies = dbContext.Currencies.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            currencies = currencies.Where(currency =>
                currency.CurrencyCode.Contains(search) || currency.CurrencyName.Contains(search));
        }

        if (query.IsActive.HasValue)
        {
            currencies = currencies.Where(currency => currency.IsActive == query.IsActive.Value);
        }

        var totalCount = await currencies.CountAsync(cancellationToken);
        var result = await currencies
            .OrderBy(currency => currency.CurrencyName)
            .ApplyCatalogPaging(query, totalCount, Response.Headers)
            .Select(currency => ToResponse(currency))
            .ToListAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CurrencyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CurrencyResponse>> GetCurrency(int id, CancellationToken cancellationToken)
    {
        var currency = await dbContext.Currencies
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.CurrencyId == id, cancellationToken);

        return currency is null ? NotFound() : Ok(ToResponse(currency));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CurrencyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CurrencyResponse>> CreateCurrency(
        CurrencyCreateRequest request,
        CancellationToken cancellationToken)
    {
        var code = request.CurrencyCode.Trim().ToUpperInvariant();
        var name = request.CurrencyName.Trim();
        if (await dbContext.Currencies.AnyAsync(item => item.CurrencyCode == code, cancellationToken))
        {
            return CurrencyConflict();
        }

        var currency = new Currency
        {
            CurrencyCode = code,
            CurrencyName = name,
            IsActive = request.IsActive ?? true
        };

        dbContext.Currencies.Add(currency);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_Currencies_CurrencyCode"))
        {
            return CurrencyConflict();
        }

        return CreatedAtAction(
            nameof(GetCurrency),
            new { id = currency.CurrencyId },
            ToResponse(currency));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CurrencyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CurrencyResponse>> UpdateCurrency(
        int id,
        CurrencyUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var currency = await dbContext.Currencies
            .SingleOrDefaultAsync(item => item.CurrencyId == id, cancellationToken);
        if (currency is null)
        {
            return NotFound();
        }

        var code = request.CurrencyCode.Trim().ToUpperInvariant();
        if (await dbContext.Currencies.AnyAsync(
                item => item.CurrencyId != id && item.CurrencyCode == code,
                cancellationToken))
        {
            return CurrencyConflict();
        }

        currency.CurrencyCode = code;
        currency.CurrencyName = request.CurrencyName.Trim();
        currency.IsActive = request.IsActive;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            CatalogControllerHelpers.IsUniqueViolation(exception, "IX_Currencies_CurrencyCode"))
        {
            return CurrencyConflict();
        }

        return Ok(ToResponse(currency));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCurrency(int id, CancellationToken cancellationToken)
    {
        var currency = await dbContext.Currencies
            .SingleOrDefaultAsync(item => item.CurrencyId == id, cancellationToken);
        if (currency is null)
        {
            return NotFound();
        }

        if (await IsReferencedAsync(id, cancellationToken))
        {
            return CurrencyInUseConflict();
        }

        dbContext.Currencies.Remove(currency);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (CatalogControllerHelpers.IsForeignKeyViolation(exception))
        {
            return CurrencyInUseConflict();
        }

        return NoContent();
    }

    private async Task<bool> IsReferencedAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.ExchangeRates.AnyAsync(item => item.CurrencyId == id, cancellationToken)
            || await dbContext.PurchaseOrders.AnyAsync(item => item.CurrencyId == id, cancellationToken)
            || await dbContext.PurchasePriceHistories.AnyAsync(item => item.CurrencyId == id, cancellationToken)
            || await dbContext.SalesOrders.AnyAsync(item => item.CurrencyId == id, cancellationToken)
            || await dbContext.Payments.AnyAsync(item => item.CurrencyId == id, cancellationToken);
    }

    private static CurrencyResponse ToResponse(Currency currency)
    {
        return new CurrencyResponse(
            currency.CurrencyId,
            currency.CurrencyCode,
            currency.CurrencyName,
            currency.IsActive);
    }

    private ConflictObjectResult CurrencyConflict()
    {
        return CatalogControllerHelpers.InUseConflict("A currency with this code already exists.");
    }

    private ConflictObjectResult CurrencyInUseConflict()
    {
        return CatalogControllerHelpers.InUseConflict(
            "The currency cannot be deleted because it is referenced by exchange rates or business records.");
    }
}
