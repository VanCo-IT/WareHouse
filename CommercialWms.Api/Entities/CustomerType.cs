namespace CommercialWms.Api.Entities;

public sealed class CustomerType
{
    public int CustomerTypeId { get; set; }
    public string TypeCode { get; set; } = null!;
    public string TypeName { get; set; } = null!;
}
