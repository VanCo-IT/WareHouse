using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class GoodsReceiptDetailConfiguration : IEntityTypeConfiguration<GoodsReceiptDetail>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptDetail> builder)
    {
        builder.ToTable("GoodsReceiptDetails", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_GoodsReceiptDetails_Qty", "((([AcceptedQuantity]+[RejectedQuantity])<=[ReceivedQuantity]))");
        });
        builder.HasKey(x => x.GoodsReceiptDetailId);

        builder.Property(x => x.GoodsReceiptDetailId)
            .HasColumnName("GoodsReceiptDetailId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.GoodsReceiptDetailId).UseIdentityColumn();
        builder.Property(x => x.GoodsReceiptId)
            .HasColumnName("GoodsReceiptId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.PurchaseOrderDetailId)
            .HasColumnName("PurchaseOrderDetailId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.ProductId)
            .HasColumnName("ProductId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ReceivedQuantity)
            .HasColumnName("ReceivedQuantity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .IsRequired(true);
        builder.Property(x => x.AcceptedQuantity)
            .HasColumnName("AcceptedQuantity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .HasDefaultValueSql("((0))")
            .IsRequired(true);
        builder.Property(x => x.RejectedQuantity)
            .HasColumnName("RejectedQuantity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .HasDefaultValueSql("((0))")
            .IsRequired(true);
        builder.Property(x => x.UnitId)
            .HasColumnName("UnitId")
            .HasColumnType("int")
            .IsRequired(true);

        builder.HasOne<PurchaseOrderDetail>()
            .WithMany()
            .HasForeignKey(x => x.PurchaseOrderDetailId)
            .HasConstraintName("FK_GoodsReceiptDetails_PODetails")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .HasConstraintName("FK_GoodsReceiptDetails_Products")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<GoodsReceipt>()
            .WithMany()
            .HasForeignKey(x => x.GoodsReceiptId)
            .HasConstraintName("FK_GoodsReceiptDetails_Receipts")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Unit>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_GoodsReceiptDetails_Units")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
