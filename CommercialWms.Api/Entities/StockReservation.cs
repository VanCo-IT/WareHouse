namespace CommercialWms.Api.Entities;

public sealed class StockReservation
{
    public long ReservationId { get; set; }
    public long LotId { get; set; }
    public int BinId { get; set; }
    public long? SalesOrderDetailId { get; set; }
    public decimal ReservedQuantity { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
