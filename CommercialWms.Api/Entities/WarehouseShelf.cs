namespace CommercialWms.Api.Entities;

public sealed class WarehouseShelf
{
    public int ShelfId { get; set; }
    public int RackId { get; set; }
    public string ShelfCode { get; set; } = null!;
}
