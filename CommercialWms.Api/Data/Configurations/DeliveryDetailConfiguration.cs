using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class DeliveryDetailConfiguration : IEntityTypeConfiguration<DeliveryDetail>
{
    public void Configure(EntityTypeBuilder<DeliveryDetail> builder)
    {
        builder.ToTable("DeliveryDetails");
        builder.HasKey(x => x.DeliveryDetailId);

        builder.Property(x => x.DeliveryDetailId)
            .HasColumnName("DeliveryDetailId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.DeliveryDetailId).UseIdentityColumn();
        builder.Property(x => x.DeliveryId)
            .HasColumnName("DeliveryId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.SalesOrderDetailId)
            .HasColumnName("SalesOrderDetailId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.LotId)
            .HasColumnName("LotId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.Quantity)
            .HasColumnName("Quantity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .IsRequired(true);

        builder.HasOne<Delivery>()
            .WithMany()
            .HasForeignKey(x => x.DeliveryId)
            .HasConstraintName("FK_DeliveryDetails_Deliveries")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<InventoryLot>()
            .WithMany()
            .HasForeignKey(x => x.LotId)
            .HasConstraintName("FK_DeliveryDetails_Lots")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<SalesOrderDetail>()
            .WithMany()
            .HasForeignKey(x => x.SalesOrderDetailId)
            .HasConstraintName("FK_DeliveryDetails_SalesOrderDetails")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
