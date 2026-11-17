using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class ProductUnitConfiguration : IEntityTypeConfiguration<ProductUnit>
{
    public void Configure(EntityTypeBuilder<ProductUnit> builder)
    {
        builder.ToTable("ProductUnits");
        builder.HasKey(x => x.ProductUnitId);

        builder.Property(x => x.ProductUnitId)
            .HasColumnName("ProductUnitId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ProductUnitId).UseIdentityColumn();
        builder.Property(x => x.ProductId)
            .HasColumnName("ProductId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.UnitId)
            .HasColumnName("UnitId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ConversionToBase)
            .HasColumnName("ConversionToBase")
            .HasColumnType("decimal(18,6)")
            .HasPrecision(18, 6)
            .IsRequired(true);
        builder.Property(x => x.IsBaseUnit)
            .HasColumnName("IsBaseUnit")
            .HasColumnType("bit")
            .HasDefaultValueSql("((0))")
            .HasSentinel(false)
            .IsRequired(true);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .HasConstraintName("FK_ProductUnits_Products")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Unit>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_ProductUnits_Units")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.ProductId, x.UnitId }).IsUnique();
    }
}
