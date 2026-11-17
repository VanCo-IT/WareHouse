namespace CommercialWms.Api.Entities;

public sealed class AuditLog
{
    public long AuditLogId { get; set; }
    public int? EmployeeId { get; set; }
    public string ActionName { get; set; } = null!;
    public string EntityName { get; set; } = null!;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime CreatedAt { get; set; }
}
