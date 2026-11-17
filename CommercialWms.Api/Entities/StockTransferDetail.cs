namespace CommercialWms.Api.Entities;

public sealed class StockTransferDetail
{
    public long TransferDetailId { get; set; }
    public long TransferId { get; set; }
    public int ProductId { get; set; }
    public long LotId { get; set; }
    public int SourceBinId { get; set; }
    public int DestinationBinId { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
}
