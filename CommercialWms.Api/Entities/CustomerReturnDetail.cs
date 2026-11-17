namespace CommercialWms.Api.Entities;

public sealed class CustomerReturnDetail
{
    public long CustomerReturnDetailId { get; set; }
    public long CustomerReturnId { get; set; }
    public int ProductId { get; set; }
    public long? LotId { get; set; }
    public decimal Quantity { get; set; }
    public string? ConditionNote { get; set; }
}
