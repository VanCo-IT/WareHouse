namespace CommercialWms.Api.Entities;

public sealed class ProductUnit
{
    public int ProductUnitId { get; set; }
    public int ProductId { get; set; }
    public int UnitId { get; set; }
    public decimal ConversionToBase { get; set; }
    public bool IsBaseUnit { get; set; }
}
