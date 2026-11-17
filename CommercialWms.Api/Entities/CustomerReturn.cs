namespace CommercialWms.Api.Entities;

public sealed class CustomerReturn
{
    public long CustomerReturnId { get; set; }
    public string ReturnNumber { get; set; } = null!;
    public int CustomerId { get; set; }
    public long? SalesOrderId { get; set; }
    public DateTime ReturnedAt { get; set; }
    public string Reason { get; set; } = null!;
    public string Status { get; set; } = null!;
}
