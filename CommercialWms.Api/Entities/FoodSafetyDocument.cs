namespace CommercialWms.Api.Entities;

public sealed class FoodSafetyDocument
{
    public long FoodSafetyDocumentId { get; set; }
    public int? SupplierId { get; set; }
    public int? ManufacturerId { get; set; }
    public long? LotId { get; set; }
    public string DocumentType { get; set; } = null!;
    public string? DocumentNumber { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? FilePath { get; set; }
}
