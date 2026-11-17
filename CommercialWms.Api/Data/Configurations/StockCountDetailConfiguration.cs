using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class StockCountDetailConfiguration : IEntityTypeConfiguration<StockCountDetail>
{
    public void Configure(EntityTypeBuilder<StockCountDetail> builder)
    {
        builder.ToTable("StockCountDetails");
        builder.HasKey(x => x.StockCountDetailId);

        builder.Property(x => x.StockCountDetailId)
            .HasColumnName("StockCountDetailId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.StockCountDetailId).UseIdentityColumn();
        builder.Property(x => x.StockCountId)
            .HasColumnName("StockCountId")
            .HasColumnType("bigint")
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
            .IsRequired(false);
        builder.Property(x => x.DifferenceQuantity)
            .HasColumnName("DifferenceQuantity")
            .HasColumnType("decimal(19,3)")
            .HasPrecision(19, 3)
            .HasComputedColumnSql("([ActualQuantity]-[SystemQuantity])", stored: true)
            .IsRequired(false);

        builder.HasOne<WarehouseBin>()
            .WithMany()
            .HasForeignKey(x => x.BinId)
            .HasConstraintName("FK_StockCountDetails_Bins")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<InventoryLot>()
            .WithMany()
            .HasForeignKey(x => x.LotId)
            .HasConstraintName("FK_StockCountDetails_Lots")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<StockCount>()
            .WithMany()
            .HasForeignKey(x => x.StockCountId)
            .HasConstraintName("FK_StockCountDetails_StockCounts")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.StockCountId, x.LotId, x.BinId }).IsUnique();
    }
}
