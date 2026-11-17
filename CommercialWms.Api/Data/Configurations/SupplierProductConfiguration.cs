using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class SupplierProductConfiguration : IEntityTypeConfiguration<SupplierProduct>
{
    public void Configure(EntityTypeBuilder<SupplierProduct> builder)
    {
        builder.ToTable("SupplierProducts");
        builder.HasKey(x => x.SupplierProductId);

        builder.Property(x => x.SupplierProductId)
            .HasColumnName("SupplierProductId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.SupplierProductId).UseIdentityColumn();
        builder.Property(x => x.SupplierId)
            .HasColumnName("SupplierId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ProductId)
            .HasColumnName("ProductId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ManufacturerId)
            .HasColumnName("ManufacturerId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.SupplierProductCode)
            .HasColumnName("SupplierProductCode")
            .HasColumnType("varchar(60)")
            .HasMaxLength(60)
            .IsUnicode(false)
            .IsRequired(false);
        builder.Property(x => x.LeadTimeDays)
            .HasColumnName("LeadTimeDays")
            .HasColumnType("int")
            .HasDefaultValueSql("((0))")
            .IsRequired(true);
        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("bit")
            .HasDefaultValueSql("((1))")
            .HasSentinel(true)
            .IsRequired(true);

        builder.HasOne<Manufacturer>()
            .WithMany()
            .HasForeignKey(x => x.ManufacturerId)
            .HasConstraintName("FK_SupplierProducts_Manufacturers")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .HasConstraintName("FK_SupplierProducts_Products")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .HasConstraintName("FK_SupplierProducts_Suppliers")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.SupplierId, x.ProductId, x.ManufacturerId }).IsUnique();
    }
}
