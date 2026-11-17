namespace CommercialWms.Api.Entities;

public sealed class WarehouseTemperatureLog
{
    public long TemperatureLogId { get; set; }
    public int WarehouseId { get; set; }
    public DateTime RecordedAt { get; set; }
    public decimal TemperatureC { get; set; }
    public int? RecordedByEmployeeId { get; set; }
    public string? Note { get; set; }
}
