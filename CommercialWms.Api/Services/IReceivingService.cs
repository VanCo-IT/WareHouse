using CommercialWms.Api.Contracts.Receiving;

namespace CommercialWms.Api.Services;

public interface IReceivingService
{
    Task<IReadOnlyList<PurchaseOrderResponse>> GetPurchaseOrdersAsync(string? status, CancellationToken cancellationToken);
    Task<PurchaseOrderResponse> GetPurchaseOrderAsync(long id, CancellationToken cancellationToken);
    Task<PurchaseOrderResponse> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken);
    Task<PurchaseOrderResponse> SubmitPurchaseOrderAsync(long id, CancellationToken cancellationToken);
    Task<PurchaseOrderResponse> ApprovePurchaseOrderAsync(long id, CancellationToken cancellationToken);
    Task<PurchaseOrderResponse> CancelPurchaseOrderAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductUnitOptionResponse>> GetProductUnitsAsync(int productId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ReceivingBinOptionResponse>> GetWarehouseBinsAsync(int warehouseId, CancellationToken cancellationToken);
    Task<IReadOnlyList<GoodsReceiptListItemResponse>> GetGoodsReceiptsAsync(
        string? search,
        string? status,
        int? supplierId,
        int? warehouseId,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken);
    Task<GoodsReceiptResponse> GetGoodsReceiptAsync(long id, CancellationToken cancellationToken);
    Task<GoodsReceiptResponse> CreateGoodsReceiptAsync(CreateGoodsReceiptRequest request, CancellationToken cancellationToken);
    Task<GoodsReceiptResponse> SaveCheckInAsync(long id, GoodsReceiptCheckInRequest request, CancellationToken cancellationToken);
    Task<GoodsReceiptResponse> SaveQualityInspectionAsync(long id, SaveQualityInspectionRequest request, CancellationToken cancellationToken);
    Task<GoodsReceiptResponse> ConfirmGoodsReceiptAsync(long id, ConfirmGoodsReceiptRequest request, CancellationToken cancellationToken);
    Task<GoodsReceiptResponse> CancelGoodsReceiptAsync(long id, CancellationToken cancellationToken);
}
