using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class ProductWarehousePolicyConfiguration : IEntityTypeConfiguration<ProductWarehousePolicy>
{
    public void Configure(EntityTypeBuilder<ProductWarehousePolicy> builder)
    {
        builder.ToTable("ProductWarehousePolicies", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_ProductWarehousePolicies_Max", "(([MaxQuantity] IS NULL OR [MaxQuantity]>=[MinQuantity]))");
        });
        builder.HasKey(x => x.PolicyId);

        builder.Property(x => x.PolicyId)
            .HasColumnName("PolicyId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.PolicyId).UseIdentityColumn();
        builder.Property(x => x.ProductId)
            .HasColumnName("ProductId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.WarehouseId)
            .HasColumnName("WarehouseId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.MinQuantity)
            .HasColumnName("MinQuantity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .HasDefaultValueSql("((0))")
            .IsRequired(true);
        builder.Property(x => x.MaxQuantity)
            .HasColumnName("MaxQuantity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .IsRequired(false);
        builder.Property(x => x.SafetyQuantity)
            .HasColumnName("SafetyQuantity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .HasDefaultValueSql("((0))")
            .IsRequired(true);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .HasConstraintName("FK_ProductWarehousePolicies_Products")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.ProductId, x.WarehouseId }).IsUnique();
    }
}
