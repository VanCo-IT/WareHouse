namespace CommercialWms.Api.Entities;

public sealed class Unit
{
    public int UnitId { get; set; }
    public string UnitCode { get; set; } = null!;
    public string UnitName { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}
