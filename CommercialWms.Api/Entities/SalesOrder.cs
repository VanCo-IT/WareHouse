namespace CommercialWms.Api.Entities;

public sealed class SalesOrder
{
    public long SalesOrderId { get; set; }
    public string SalesOrderNumber { get; set; } = null!;
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public int CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; }
    public string Status { get; set; } = null!;
    public int? CreatedByEmployeeId { get; set; }
    public string? Note { get; set; }
}
