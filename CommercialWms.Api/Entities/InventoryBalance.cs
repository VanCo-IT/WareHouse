namespace CommercialWms.Api.Entities;

public sealed class InventoryBalance
{
    public long BalanceId { get; set; }
    public long LotId { get; set; }
    public int BinId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public DateTime UpdatedAt { get; set; }
}
