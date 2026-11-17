namespace CommercialWms.Api.Entities;

public sealed class ManufacturerDocument
{
    public long DocumentId { get; set; }
    public int ManufacturerId { get; set; }
    public string DocumentType { get; set; } = null!;
    public string? DocumentNumber { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? FilePath { get; set; }
}
