namespace CommercialWms.Api.Entities;

public sealed class Employee
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public bool IsActive { get; set; } = true;
}
