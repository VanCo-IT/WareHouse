namespace CommercialWms.Api.Entities;

public sealed class ProductStorageRequirement
{
    public int RequirementId { get; set; }
    public int ProductId { get; set; }
    public decimal? MinTemperatureC { get; set; }
    public decimal? MaxTemperatureC { get; set; }
    public string? StorageNote { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}
