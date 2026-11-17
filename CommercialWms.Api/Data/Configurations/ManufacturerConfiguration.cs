using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class ManufacturerConfiguration : IEntityTypeConfiguration<Manufacturer>
{
    public void Configure(EntityTypeBuilder<Manufacturer> builder)
    {
        builder.ToTable("Manufacturers");
        builder.HasKey(x => x.ManufacturerId);

        builder.Property(x => x.ManufacturerId)
            .HasColumnName("ManufacturerId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ManufacturerId).UseIdentityColumn();
        builder.Property(x => x.ManufacturerCode)
            .HasColumnName("ManufacturerCode")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.ManufacturerName)
            .HasColumnName("ManufacturerName")
            .HasColumnType("nvarchar(180)")
            .HasMaxLength(180)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.CountryId)
            .HasColumnName("CountryId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.Address)
            .HasColumnName("Address")
            .HasColumnType("nvarchar(300)")
            .HasMaxLength(300)
            .IsUnicode(true)
            .IsRequired(false);
        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("bit")
            .HasDefaultValueSql("((1))")
            .HasSentinel(true)
            .IsRequired(true);

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .HasConstraintName("FK_Manufacturers_Countries")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.ManufacturerCode).IsUnique();
        builder.HasIndex(x => x.CountryId).HasDatabaseName("IX_Manufacturers_CountryId");
    }
}
