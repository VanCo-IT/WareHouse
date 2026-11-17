namespace CommercialWms.Api.Entities;

public sealed class GoodsReceipt
{
    public long GoodsReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = null!;
    public long PurchaseOrderId { get; set; }
    public int WarehouseId { get; set; }
    public DateTime ReceivedAt { get; set; }
    public int? ReceivedByEmployeeId { get; set; }
    public string Status { get; set; } = null!;
    public string? Note { get; set; }
}
