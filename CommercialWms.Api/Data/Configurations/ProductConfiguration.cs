using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(x => x.ProductId);

        builder.Property(x => x.ProductId)
            .HasColumnName("ProductId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ProductId).UseIdentityColumn();
        builder.Property(x => x.SKU)
            .HasColumnName("SKU")
            .HasColumnType("varchar(40)")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.ProductName)
            .HasColumnName("ProductName")
            .HasColumnType("nvarchar(180)")
            .HasMaxLength(180)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.CategoryId)
            .HasColumnName("CategoryId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.BrandId)
            .HasColumnName("BrandId")
            .HasColumnType("int")
            .IsRequired(false);
        builder.Property(x => x.Barcode)
            .HasColumnName("Barcode")
            .HasColumnType("varchar(60)")
            .HasMaxLength(60)
            .IsUnicode(false)
            .IsRequired(false);
        builder.Property(x => x.Description)
            .HasColumnName("Description")
            .HasColumnType("nvarchar(500)")
            .HasMaxLength(500)
            .IsUnicode(true)
            .IsRequired(false);
        builder.Property(x => x.IsPerishable)
            .HasColumnName("IsPerishable")
            .HasColumnType("bit")
            .HasDefaultValueSql("((1))")
            .HasSentinel(true)
            .IsRequired(true);
        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("bit")
            .HasDefaultValueSql("((1))")
            .HasSentinel(true)
            .IsRequired(true);
        builder.Property(x => x.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);

        builder.HasOne<Brand>()
            .WithMany()
            .HasForeignKey(x => x.BrandId)
            .HasConstraintName("FK_Products_Brands")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<ProductCategory>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .HasConstraintName("FK_Products_Categories")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.Barcode).IsUnique();
        builder.HasIndex(x => x.SKU).IsUnique();
        builder.HasIndex(x => x.CategoryId).HasDatabaseName("IX_Products_CategoryId");
    }
}
