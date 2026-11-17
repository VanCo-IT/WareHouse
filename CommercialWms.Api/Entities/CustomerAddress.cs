namespace CommercialWms.Api.Entities;

public sealed class CustomerAddress
{
    public int AddressId { get; set; }
    public int CustomerId { get; set; }
    public string AddressLine { get; set; } = null!;
    public string? Province { get; set; }
    public string? District { get; set; }
    public bool IsDefault { get; set; }
}
