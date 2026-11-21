using CommercialWms.Api.Contracts.Outbound;

namespace CommercialWms.Api.Services;

public interface IOutboundService
{
    Task<OutboundOptionsResponse> GetOptionsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<OutboundAvailableStockResponse>> GetAvailableStockAsync(
        int warehouseId,
        int productId,
        int unitId,
        CancellationToken cancellationToken);

    Task<OutboundPageResponse<OutboundOrderListItemResponse>> GetOrdersAsync(
        string? search,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<OutboundOrderResponse> GetOrderAsync(long id, CancellationToken cancellationToken);

    Task<OutboundOrderResponse> CreateOrderAsync(
        CreateOutboundOrderRequest request,
        CancellationToken cancellationToken);

    Task<OutboundOrderResponse> ConfirmOrderAsync(
        long id,
        ConfirmOutboundOrderRequest request,
        CancellationToken cancellationToken);

    Task<OutboundOrderResponse> CancelOrderAsync(long id, CancellationToken cancellationToken);

    Task<OutboundPageResponse<OutboundDeliveryListItemResponse>> GetDeliveriesAsync(
        string? search,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<OutboundDeliveryResponse> GetDeliveryAsync(long id, CancellationToken cancellationToken);

    Task<OutboundDeliveryResponse> PrepareDeliveryAsync(long id, CancellationToken cancellationToken);

    Task<OutboundDeliveryResponse> ShipDeliveryAsync(
        long id,
        ShipOutboundDeliveryRequest request,
        CancellationToken cancellationToken);

    Task<OutboundDeliveryResponse> MarkDeliveredAsync(long id, CancellationToken cancellationToken);

    Task<OutboundDeliveryResponse> CancelDeliveryAsync(long id, CancellationToken cancellationToken);
}
