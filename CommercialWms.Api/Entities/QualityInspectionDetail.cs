namespace CommercialWms.Api.Entities;

public sealed class QualityInspectionDetail
{
    public long QualityInspectionDetailId { get; set; }
    public long QualityInspectionId { get; set; }
    public long GoodsReceiptDetailId { get; set; }
    public string CheckName { get; set; } = null!;
    public string Result { get; set; } = null!;
    public string? Note { get; set; }
}
