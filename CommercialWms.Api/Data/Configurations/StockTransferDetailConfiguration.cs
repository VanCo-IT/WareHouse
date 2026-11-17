using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class StockTransferDetailConfiguration : IEntityTypeConfiguration<StockTransferDetail>
{
    public void Configure(EntityTypeBuilder<StockTransferDetail> builder)
    {
        builder.ToTable("StockTransferDetails", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_StockTransferDetails_Received", "(([ReceivedQuantity]<=[RequestedQuantity]))");
        });
        builder.HasKey(x => x.TransferDetailId);

        builder.Property(x => x.TransferDetailId)
            .HasColumnName("TransferDetailId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.TransferDetailId).UseIdentityColumn();
        builder.Property(x => x.TransferId)
            .HasColumnName("TransferId")
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
        builder.Property(x => x.SourceBinId)
            .HasColumnName("SourceBinId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.DestinationBinId)
            .HasColumnName("DestinationBinId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.RequestedQuantity)
            .HasColumnName("RequestedQuantity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .IsRequired(true);
        builder.Property(x => x.ReceivedQuantity)
            .HasColumnName("ReceivedQuantity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .HasDefaultValueSql("((0))")
            .IsRequired(true);

        builder.HasOne<WarehouseBin>()
            .WithMany()
            .HasForeignKey(x => x.DestinationBinId)
            .HasConstraintName("FK_StockTransferDetails_DestinationBins")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<InventoryLot>()
            .WithMany()
            .HasForeignKey(x => x.LotId)
            .HasConstraintName("FK_StockTransferDetails_Lots")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .HasConstraintName("FK_StockTransferDetails_Products")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<WarehouseBin>()
            .WithMany()
            .HasForeignKey(x => x.SourceBinId)
            .HasConstraintName("FK_StockTransferDetails_SourceBins")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<StockTransfer>()
            .WithMany()
            .HasForeignKey(x => x.TransferId)
            .HasConstraintName("FK_StockTransferDetails_Transfers")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
