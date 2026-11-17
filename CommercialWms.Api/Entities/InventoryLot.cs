namespace CommercialWms.Api.Entities;

public sealed class InventoryLot
{
    public long LotId { get; set; }
    public string LotCode { get; set; } = null!;
    public int ProductId { get; set; }
    public int ManufacturerId { get; set; }
    public int CountryId { get; set; }
    public DateOnly? ManufactureDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? SupplierLotCode { get; set; }
    public DateTime CreatedAt { get; set; }
}
