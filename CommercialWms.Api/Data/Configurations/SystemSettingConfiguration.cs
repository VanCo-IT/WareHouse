using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");
        builder.HasKey(x => x.SettingId);

        builder.Property(x => x.SettingId)
            .HasColumnName("SettingId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.SettingId).UseIdentityColumn();
        builder.Property(x => x.SettingKey)
            .HasColumnName("SettingKey")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.SettingValue)
            .HasColumnName("SettingValue")
            .HasColumnType("nvarchar(500)")
            .HasMaxLength(500)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.Description)
            .HasColumnName("Description")
            .HasColumnType("nvarchar(300)")
            .HasMaxLength(300)
            .IsUnicode(true)
            .IsRequired(false);
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);


        builder.HasIndex(x => x.SettingKey).IsUnique();
    }
}
