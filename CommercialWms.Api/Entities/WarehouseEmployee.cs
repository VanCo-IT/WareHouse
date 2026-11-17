namespace CommercialWms.Api.Entities;

public sealed class WarehouseEmployee
{
    public long WarehouseEmployeeId { get; set; }
    public int WarehouseId { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly AssignedFrom { get; set; }
    public DateOnly? AssignedTo { get; set; }
}
