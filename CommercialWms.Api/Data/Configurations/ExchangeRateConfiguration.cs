using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("ExchangeRates");
        builder.HasKey(x => x.ExchangeRateId);

        builder.Property(x => x.ExchangeRateId)
            .HasColumnName("ExchangeRateId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.ExchangeRateId).UseIdentityColumn();
        builder.Property(x => x.CurrencyId)
            .HasColumnName("CurrencyId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.RateDate)
            .HasColumnName("RateDate")
            .HasColumnType("date")
            .IsRequired(true);
        builder.Property(x => x.RateToVND)
            .HasColumnName("RateToVND")
            .HasColumnType("decimal(18,6)")
            .HasPrecision(18, 6)
            .IsRequired(true);
        builder.Property(x => x.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .HasConstraintName("FK_ExchangeRates_Currencies")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.CurrencyId, x.RateDate }).IsUnique();
    }
}
