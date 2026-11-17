namespace CommercialWms.Api.Entities;

public sealed class Delivery
{
    public long DeliveryId { get; set; }
    public string DeliveryNumber { get; set; } = null!;
    public long SalesOrderId { get; set; }
    public int WarehouseId { get; set; }
    public int? DeliveryAddressId { get; set; }
    public DateTime? PlannedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public string Status { get; set; } = null!;
}
