using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");
        builder.HasKey(x => x.CountryId);

        builder.Property(x => x.CountryId)
            .HasColumnName("CountryId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.CountryId).UseIdentityColumn();
        builder.Property(x => x.CountryCode)
            .HasColumnName("CountryCode")
            .HasColumnType("char(2)")
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength()
            .IsRequired(true);
        builder.Property(x => x.CountryName)
            .HasColumnName("CountryName")
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("bit")
            .HasDefaultValueSql("((1))")
            .HasSentinel(true)
            .IsRequired(true);


        builder.HasIndex(x => x.CountryCode).IsUnique();
        builder.HasIndex(x => x.CountryName).IsUnique();

        builder.HasData(new Country { CountryId = 1, CountryCode = "VN", CountryName = "Vietnam", IsActive = true });
    }
}
