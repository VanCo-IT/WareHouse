using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");
        builder.HasKey(x => x.CurrencyId);

        builder.Property(x => x.CurrencyId)
            .HasColumnName("CurrencyId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.CurrencyId).UseIdentityColumn();
        builder.Property(x => x.CurrencyCode)
            .HasColumnName("CurrencyCode")
            .HasColumnType("char(3)")
            .HasMaxLength(3)
            .IsUnicode(false)
            .IsFixedLength()
            .IsRequired(true);
        builder.Property(x => x.CurrencyName)
            .HasColumnName("CurrencyName")
            .HasColumnType("nvarchar(80)")
            .HasMaxLength(80)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("bit")
            .HasDefaultValueSql("((1))")
            .HasSentinel(true)
            .IsRequired(true);


        builder.HasIndex(x => x.CurrencyCode).IsUnique();

        builder.HasData(new Currency { CurrencyId = 1, CurrencyCode = "VND", CurrencyName = "Vietnamese Dong", IsActive = true });
    }
}
