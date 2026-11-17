using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class StockAdjustmentDetailConfiguration : IEntityTypeConfiguration<StockAdjustmentDetail>
{
    public void Configure(EntityTypeBuilder<StockAdjustmentDetail> builder)
    {
        builder.ToTable("StockAdjustmentDetails");
        builder.HasKey(x => x.AdjustmentDetailId);

        builder.Property(x => x.AdjustmentDetailId)
            .HasColumnName("AdjustmentDetailId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.AdjustmentDetailId).UseIdentityColumn();
        builder.Property(x => x.AdjustmentId)
            .HasColumnName("AdjustmentId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.ProductId)
            .HasColumnName("ProductId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.LotId)
            .HasColumnName("LotId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.BinId)
            .HasColumnName("BinId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.SystemQuantity)
            .HasColumnName("SystemQuantity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .IsRequired(true);
        builder.Property(x => x.ActualQuantity)
            .HasColumnName("ActualQuantity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .IsRequired(true);
        builder.Property(x => x.DifferenceQuantity)
            .HasColumnName("DifferenceQuantity")
            .HasColumnType("decimal(19,3)")
            .HasPrecision(19, 3)
            .HasComputedColumnSql("([ActualQuantity]-[SystemQuantity])", stored: true)
            .IsRequired(false);

        builder.HasOne<StockAdjustment>()
            .WithMany()
            .HasForeignKey(x => x.AdjustmentId)
            .HasConstraintName("FK_StockAdjustmentDetails_Adjustments")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<WarehouseBin>()
            .WithMany()
            .HasForeignKey(x => x.BinId)
            .HasConstraintName("FK_StockAdjustmentDetails_Bins")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<InventoryLot>()
            .WithMany()
            .HasForeignKey(x => x.LotId)
            .HasConstraintName("FK_StockAdjustmentDetails_Lots")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .HasConstraintName("FK_StockAdjustmentDetails_Products")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
