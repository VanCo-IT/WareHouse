using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class PurchaseOrderDetailConfiguration : IEntityTypeConfiguration<PurchaseOrderDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderDetail> builder)
    {
        builder.ToTable("PurchaseOrderDetails");
        builder.HasKey(x => x.PurchaseOrderDetailId);

        builder.Property(x => x.PurchaseOrderDetailId)
            .HasColumnName("PurchaseOrderDetailId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.PurchaseOrderDetailId).UseIdentityColumn();
        builder.Property(x => x.PurchaseOrderId)
            .HasColumnName("PurchaseOrderId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.ProductId)
            .HasColumnName("ProductId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.OrderedQuantity)
            .HasColumnName("OrderedQuantity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .IsRequired(true);
        builder.Property(x => x.UnitId)
            .HasColumnName("UnitId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.UnitPrice)
            .HasColumnName("UnitPrice")
            .HasColumnType("decimal(18,4)")
            .HasPrecision(18, 4)
            .IsRequired(true);

        builder.HasOne<PurchaseOrder>()
            .WithMany()
            .HasForeignKey(x => x.PurchaseOrderId)
            .HasConstraintName("FK_PurchaseOrderDetails_Orders")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .HasConstraintName("FK_PurchaseOrderDetails_Products")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Unit>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_PurchaseOrderDetails_Units")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
