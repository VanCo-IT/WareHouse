namespace CommercialWms.Api.Entities;

public sealed class StockTransfer
{
    public long TransferId { get; set; }
    public string TransferNumber { get; set; } = null!;
    public int SourceWarehouseId { get; set; }
    public int DestinationWarehouseId { get; set; }
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = null!;
    public int? RequestedByEmployeeId { get; set; }
    public string? Note { get; set; }
}
