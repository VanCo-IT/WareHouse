using CommercialWms.Api.Contracts.Outbound;
using CommercialWms.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommercialWms.Api.Controllers;

[ApiController]
[Route("api/outbound")]
public sealed class OutboundController(IOutboundService outboundService) : ControllerBase
{
    [HttpGet("options")]
    [ProducesResponseType(typeof(OutboundOptionsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OutboundOptionsResponse>> GetOptions(CancellationToken cancellationToken)
    {
        return await Run(() => outboundService.GetOptionsAsync(cancellationToken));
    }

    [HttpGet("availability")]
    [ProducesResponseType(typeof(IReadOnlyList<OutboundAvailableStockResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<OutboundAvailableStockResponse>>> GetAvailability(
        [FromQuery] int warehouseId,
        [FromQuery] int productId,
        [FromQuery] int unitId,
        CancellationToken cancellationToken)
    {
        return await Run(() => outboundService.GetAvailableStockAsync(
            warehouseId,
            productId,
            unitId,
            cancellationToken));
    }

    [HttpGet("orders")]
    [ProducesResponseType(typeof(OutboundPageResponse<OutboundOrderListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OutboundPageResponse<OutboundOrderListItemResponse>>> GetOrders(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        return await Run(() => outboundService.GetOrdersAsync(
            search,
            status,
            page,
            pageSize,
            cancellationToken));
    }

    [HttpGet("orders/{id:long}")]
    [ProducesResponseType(typeof(OutboundOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutboundOrderResponse>> GetOrder(
        long id,
        CancellationToken cancellationToken)
    {
        return await Run(() => outboundService.GetOrderAsync(id, cancellationToken));
    }

    [HttpPost("orders")]
    [ProducesResponseType(typeof(OutboundOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OutboundOrderResponse>> CreateOrder(
        [FromBody] CreateOutboundOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await outboundService.CreateOrderAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetOrder), new { id = response.SalesOrderId }, response);
        }
        catch (OutboundOperationException exception)
        {
            return ToProblem(exception);
        }
    }

    [HttpPost("orders/{id:long}/confirm")]
    [ProducesResponseType(typeof(OutboundOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutboundOrderResponse>> ConfirmOrder(
        long id,
        [FromBody] ConfirmOutboundOrderRequest request,
        CancellationToken cancellationToken)
    {
        return await Run(() => outboundService.ConfirmOrderAsync(id, request, cancellationToken));
    }

    [HttpPost("orders/{id:long}/cancel")]
    [ProducesResponseType(typeof(OutboundOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutboundOrderResponse>> CancelOrder(
        long id,
        CancellationToken cancellationToken)
    {
        return await Run(() => outboundService.CancelOrderAsync(id, cancellationToken));
    }

    [HttpGet("deliveries")]
    [ProducesResponseType(typeof(OutboundPageResponse<OutboundDeliveryListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OutboundPageResponse<OutboundDeliveryListItemResponse>>> GetDeliveries(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        return await Run(() => outboundService.GetDeliveriesAsync(
            search,
            status,
            page,
            pageSize,
            cancellationToken));
    }

    [HttpGet("deliveries/{id:long}")]
    [ProducesResponseType(typeof(OutboundDeliveryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutboundDeliveryResponse>> GetDelivery(
        long id,
        CancellationToken cancellationToken)
    {
        return await Run(() => outboundService.GetDeliveryAsync(id, cancellationToken));
    }

    [HttpPost("deliveries/{id:long}/prepare")]
    [ProducesResponseType(typeof(OutboundDeliveryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutboundDeliveryResponse>> PrepareDelivery(
        long id,
        CancellationToken cancellationToken)
    {
        return await Run(() => outboundService.PrepareDeliveryAsync(id, cancellationToken));
    }

    [HttpPost("deliveries/{id:long}/ship")]
    [ProducesResponseType(typeof(OutboundDeliveryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutboundDeliveryResponse>> ShipDelivery(
        long id,
        [FromBody] ShipOutboundDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        return await Run(() => outboundService.ShipDeliveryAsync(id, request, cancellationToken));
    }

    [HttpPost("deliveries/{id:long}/delivered")]
    [ProducesResponseType(typeof(OutboundDeliveryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutboundDeliveryResponse>> MarkDelivered(
        long id,
        CancellationToken cancellationToken)
    {
        return await Run(() => outboundService.MarkDeliveredAsync(id, cancellationToken));
    }

    [HttpPost("deliveries/{id:long}/cancel")]
    [ProducesResponseType(typeof(OutboundDeliveryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutboundDeliveryResponse>> CancelDelivery(
        long id,
        CancellationToken cancellationToken)
    {
        return await Run(() => outboundService.CancelDeliveryAsync(id, cancellationToken));
    }

    private async Task<ActionResult<T>> Run<T>(Func<Task<T>> operation)
    {
        try
        {
            return Ok(await operation());
        }
        catch (OutboundOperationException exception)
        {
            return ToProblem(exception);
        }
    }

    private ObjectResult ToProblem(OutboundOperationException exception)
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
