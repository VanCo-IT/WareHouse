using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(x => x.SupplierId);

        builder.Property(x => x.SupplierId)
            .HasColumnName("SupplierId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.SupplierId).UseIdentityColumn();
        builder.Property(x => x.SupplierCode)
            .HasColumnName("SupplierCode")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.SupplierName)
            .HasColumnName("SupplierName")
            .HasColumnType("nvarchar(180)")
            .HasMaxLength(180)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.CountryId)
            .HasColumnName("CountryId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.TaxCode)
            .HasColumnName("TaxCode")
            .HasColumnType("varchar(40)")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired(false);
        builder.Property(x => x.Email)
            .HasColumnName("Email")
            .HasColumnType("varchar(150)")
            .HasMaxLength(150)
            .IsUnicode(false)
            .IsRequired(false);
        builder.Property(x => x.Phone)
            .HasColumnName("Phone")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(false);
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
            .HasConstraintName("FK_Suppliers_Countries")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.SupplierCode).IsUnique();
        builder.HasIndex(x => x.CountryId).HasDatabaseName("IX_Suppliers_CountryId");
    }
}
