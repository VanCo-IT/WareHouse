using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Inventory;

/// <summary>A page of current inventory balances.</summary>
public sealed record InventoryStockPageResponse(
    IReadOnlyList<InventoryStockResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    decimal TotalOnHand,
    decimal TotalReserved,
    decimal TotalAvailable);

/// <summary>A stock balance for one product lot at one warehouse bin.</summary>
public sealed record InventoryStockResponse(
    long BalanceId,
    long LotId,
    string LotCode,
    int ProductId,
    string SKU,
    string ProductName,
    int WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    int BinId,
    string BinCode,
    string LocationCode,
    string ManufacturerName,
    string CountryName,
    DateOnly? ManufactureDate,
    DateOnly? ExpiryDate,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable,
    DateTimeOffset UpdatedAt);

/// <summary>A page of inventory movement history.</summary>
public sealed record InventoryTransactionPageResponse(
    IReadOnlyList<InventoryTransactionResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);

/// <summary>An inventory movement ledger entry.</summary>
public sealed record InventoryTransactionResponse(
    long TransactionId,
    long LotId,
    string LotCode,
    int ProductId,
    string SKU,
    string ProductName,
    int WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    int? BinId,
    string? BinCode,
    string TransactionType,
    decimal QuantityChange,
    string? ReferenceType,
    long? ReferenceId,
    DateTimeOffset OccurredAt,
    string? Note);

/// <summary>Payload for a reasoned stock adjustment.</summary>
public sealed record CreateInventoryAdjustmentRequest
{
    /// <summary>Existing inventory lot to adjust.</summary>
    [Range(1, long.MaxValue)]
    public long LotId { get; init; }

    /// <summary>Active bin containing the adjusted stock.</summary>
    [Range(1, int.MaxValue)]
    public int BinId { get; init; }

    /// <summary>Signed quantity delta, using the product's base unit.</summary>
    public decimal QuantityChange { get; init; }

    /// <summary>Employee making the adjustment, when known.</summary>
    [Range(1, int.MaxValue)]
    public int? CreatedByEmployeeId { get; init; }

    /// <summary>Required business reason for the adjustment.</summary>
    [Required, StringLength(300, MinimumLength = 3)]
    public required string Reason { get; init; }
}

/// <summary>Payload for an atomic movement of stock between two bins.</summary>
public sealed record CreateInventoryTransferRequest
{
    /// <summary>Existing inventory lot to move.</summary>
    [Range(1, long.MaxValue)]
    public long LotId { get; init; }

    /// <summary>Source bin.</summary>
    [Range(1, int.MaxValue)]
    public int SourceBinId { get; init; }

    /// <summary>Destination bin.</summary>
    [Range(1, int.MaxValue)]
    public int DestinationBinId { get; init; }

    /// <summary>Positive quantity to move, in the product's base unit.</summary>
    [Range(typeof(decimal), "0.001", "999999999999999.999")]
    public decimal Quantity { get; init; }

    /// <summary>Employee making the transfer, when known.</summary>
    [Range(1, int.MaxValue)]
    public int? CreatedByEmployeeId { get; init; }

    /// <summary>Required business reason for the transfer.</summary>
    [Required, StringLength(300, MinimumLength = 3)]
    public required string Reason { get; init; }
}
