namespace CommercialWms.Api.Entities;

public sealed class InventoryTransaction
{
    public long TransactionId { get; set; }
    public long LotId { get; set; }
    public int WarehouseId { get; set; }
    public int? BinId { get; set; }
    public string TransactionType { get; set; } = null!;
    public decimal QuantityChange { get; set; }
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int? CreatedByEmployeeId { get; set; }
    public string? Note { get; set; }
}
