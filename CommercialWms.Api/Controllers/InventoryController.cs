using CommercialWms.Api.Contracts.Inventory;
using CommercialWms.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(InventoryStockPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InventoryStockPageResponse>> GetStock(
        [FromQuery] string? search,
        [FromQuery] int? warehouseId,
        [FromQuery] int? productId,
        [FromQuery] int? binId,
        [FromQuery] bool availableOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        return await Run(() => inventoryService.GetStockAsync(
            search,
            warehouseId,
            productId,
            binId,
            availableOnly,
            page,
            pageSize,
            cancellationToken));
    }

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(InventoryTransactionPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InventoryTransactionPageResponse>> GetTransactions(
        [FromQuery] int? warehouseId,
        [FromQuery] int? productId,
        [FromQuery] string? transactionType,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        return await Run(() => inventoryService.GetTransactionsAsync(
            warehouseId,
            productId,
            transactionType,
            fromDate,
            toDate,
            page,
            pageSize,
            cancellationToken));
    }

    [HttpPost("adjustments")]
    [ProducesResponseType(typeof(InventoryTransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryTransactionResponse>> CreateAdjustment(
        [FromBody] CreateInventoryAdjustmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await inventoryService.CreateAdjustmentAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetTransactions), new { page = 1, pageSize = 25 }, response);
        }
        catch (InventoryOperationException exception)
        {
            return ToProblem(exception);
        }
    }

    [HttpPost("transfers")]
    [ProducesResponseType(typeof(IReadOnlyList<InventoryTransactionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<InventoryTransactionResponse>>> CreateTransfer(
        [FromBody] CreateInventoryTransferRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await inventoryService.CreateTransferAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetTransactions), new { page = 1, pageSize = 25 }, response);
        }
        catch (InventoryOperationException exception)
        {
            return ToProblem(exception);
        }
    }

    private async Task<ActionResult<T>> Run<T>(Func<Task<T>> operation)
    {
        try
        {
            return Ok(await operation());
        }
        catch (InventoryOperationException exception)
        {
            return ToProblem(exception);
        }
    }

    private ObjectResult ToProblem(InventoryOperationException exception)
    {
        return new ObjectResult(new ProblemDetails
        {
            Status = exception.StatusCode,
            Title = exception.Title,
            Detail = exception.Message,
            Instance = HttpContext.Request.Path
        })
        {
            StatusCode = exception.StatusCode
        };
    }
}
