namespace CommercialWms.Api.Entities;

public sealed class SystemSetting
{
    public int SettingId { get; set; }
    public string SettingKey { get; set; } = null!;
    public string SettingValue { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; }
}
