using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");
        builder.HasKey(x => x.BrandId);

        builder.Property(x => x.BrandId)
            .HasColumnName("BrandId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.BrandId).UseIdentityColumn();
        builder.Property(x => x.BrandName)
            .HasColumnName("BrandName")
            .HasColumnType("nvarchar(120)")
            .HasMaxLength(120)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.CountryId)
            .HasColumnName("CountryId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("bit")
            .HasDefaultValueSql("((1))")
            .HasSentinel(true)
            .IsRequired(true);

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .HasConstraintName("FK_Brands_Countries")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.BrandName, x.CountryId })
            .IsUnique()
            .HasFilter(null);
    }
}
