using CommercialWms.Api.Contracts.Receiving;
using CommercialWms.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReceivingController(IReceivingService receivingService) : ControllerBase
{
    [HttpGet("purchase-orders")]
    [ProducesResponseType(typeof(IReadOnlyList<PurchaseOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<ActionResult<IReadOnlyList<PurchaseOrderResponse>>> GetPurchaseOrders(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        return Run(() => receivingService.GetPurchaseOrdersAsync(status, cancellationToken));
    }

    [HttpGet("purchase-orders/{id:long}")]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<PurchaseOrderResponse>> GetPurchaseOrder(
        long id,
        CancellationToken cancellationToken)
    {
        return Run(() => receivingService.GetPurchaseOrderAsync(id, cancellationToken));
    }

    [HttpPost("purchase-orders")]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PurchaseOrderResponse>> CreatePurchaseOrder(
        [FromBody] CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await receivingService.CreatePurchaseOrderAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetPurchaseOrder), new { id = response.PurchaseOrderId }, response);
        }
        catch (ReceivingOperationException exception)
        {
            return ToProblem(exception);
        }
    }

    [HttpPost("purchase-orders/{id:long}/submit")]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<PurchaseOrderResponse>> SubmitPurchaseOrder(
        long id,
        CancellationToken cancellationToken)
    {
        return Run(() => receivingService.SubmitPurchaseOrderAsync(id, cancellationToken));
    }

    [HttpPost("purchase-orders/{id:long}/approve")]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<PurchaseOrderResponse>> ApprovePurchaseOrder(
        long id,
        CancellationToken cancellationToken)
    {
        return Run(() => receivingService.ApprovePurchaseOrderAsync(id, cancellationToken));
    }

    [HttpPost("purchase-orders/{id:long}/cancel")]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<PurchaseOrderResponse>> CancelPurchaseOrder(
        long id,
        CancellationToken cancellationToken)
    {
        return Run(() => receivingService.CancelPurchaseOrderAsync(id, cancellationToken));
    }

    [HttpGet("products/{productId:int}/units")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductUnitOptionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<IReadOnlyList<ProductUnitOptionResponse>>> GetProductUnits(
        int productId,
        CancellationToken cancellationToken)
    {
        return Run(() => receivingService.GetProductUnitsAsync(productId, cancellationToken));
    }

    [HttpGet("warehouses/{warehouseId:int}/bins")]
    [ProducesResponseType(typeof(IReadOnlyList<ReceivingBinOptionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<IReadOnlyList<ReceivingBinOptionResponse>>> GetWarehouseBins(
        int warehouseId,
        CancellationToken cancellationToken)
    {
        return Run(() => receivingService.GetWarehouseBinsAsync(warehouseId, cancellationToken));
    }

    [HttpGet("receipts")]
    [ProducesResponseType(typeof(IReadOnlyList<GoodsReceiptListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<ActionResult<IReadOnlyList<GoodsReceiptListItemResponse>>> GetGoodsReceipts(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int? supplierId,
        [FromQuery] int? warehouseId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        return Run(() => receivingService.GetGoodsReceiptsAsync(
            search,
            status,
            supplierId,
            warehouseId,
            fromDate,
            toDate,
            cancellationToken));
    }

    [HttpGet("receipts/{id:long}")]
    [ProducesResponseType(typeof(GoodsReceiptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<GoodsReceiptResponse>> GetGoodsReceipt(
        long id,
        CancellationToken cancellationToken)
    {
        return Run(() => receivingService.GetGoodsReceiptAsync(id, cancellationToken));
    }

    [HttpPost("receipts")]
    [ProducesResponseType(typeof(GoodsReceiptResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GoodsReceiptResponse>> CreateGoodsReceipt(
        [FromBody] CreateGoodsReceiptRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await receivingService.CreateGoodsReceiptAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetGoodsReceipt), new { id = response.GoodsReceiptId }, response);
        }
        catch (ReceivingOperationException exception)
        {
            return ToProblem(exception);
        }
    }

    [HttpPut("receipts/{id:long}/check-in")]
    [ProducesResponseType(typeof(GoodsReceiptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<GoodsReceiptResponse>> SaveCheckIn(
        long id,
        [FromBody] GoodsReceiptCheckInRequest request,
        CancellationToken cancellationToken)
    {
        return Run(() => receivingService.SaveCheckInAsync(id, request, cancellationToken));
    }

    [HttpPut("receipts/{id:long}/quality-inspection")]
    [ProducesResponseType(typeof(GoodsReceiptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<GoodsReceiptResponse>> SaveQualityInspection(
        long id,
        [FromBody] SaveQualityInspectionRequest request,
        CancellationToken cancellationToken)
    {
        return Run(() => receivingService.SaveQualityInspectionAsync(id, request, cancellationToken));
    }

    [HttpPost("receipts/{id:long}/confirm")]
    [ProducesResponseType(typeof(GoodsReceiptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<GoodsReceiptResponse>> ConfirmGoodsReceipt(
        long id,
        [FromBody] ConfirmGoodsReceiptRequest request,
        CancellationToken cancellationToken)
    {
        return Run(() => receivingService.ConfirmGoodsReceiptAsync(id, request, cancellationToken));
    }

    [HttpPost("receipts/{id:long}/cancel")]
    [ProducesResponseType(typeof(GoodsReceiptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<GoodsReceiptResponse>> CancelGoodsReceipt(
        long id,
        CancellationToken cancellationToken)
    {
        return Run(() => receivingService.CancelGoodsReceiptAsync(id, cancellationToken));
    }

    private async Task<ActionResult<T>> Run<T>(Func<Task<T>> operation)
    {
        try
        {
            return Ok(await operation());
        }
        catch (ReceivingOperationException exception)
        {
            return ToProblem(exception);
        }
    }

    private ObjectResult ToProblem(ReceivingOperationException exception)
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
