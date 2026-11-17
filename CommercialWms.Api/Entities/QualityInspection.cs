namespace CommercialWms.Api.Entities;

public sealed class QualityInspection
{
    public long QualityInspectionId { get; set; }
    public string InspectionNumber { get; set; } = null!;
    public long GoodsReceiptId { get; set; }
    public DateTime InspectedAt { get; set; }
    public int? InspectorEmployeeId { get; set; }
    public string Result { get; set; } = null!;
    public string? Note { get; set; }
}
