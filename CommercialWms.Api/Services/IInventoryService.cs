using CommercialWms.Api.Contracts.Inventory;

namespace CommercialWms.Api.Services;

public interface IInventoryService
{
    Task<InventoryStockPageResponse> GetStockAsync(
        string? search,
        int? warehouseId,
        int? productId,
        int? binId,
        bool availableOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<InventoryTransactionPageResponse> GetTransactionsAsync(
        int? warehouseId,
        int? productId,
        string? transactionType,
        DateOnly? fromDate,
        DateOnly? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<InventoryTransactionResponse> CreateAdjustmentAsync(
        CreateInventoryAdjustmentRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<InventoryTransactionResponse>> CreateTransferAsync(
        CreateInventoryTransferRequest request,
        CancellationToken cancellationToken);
}
