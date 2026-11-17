using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("StockReservations", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_StockReservations_Status", "(([Status]='Cancelled' OR [Status]='Fulfilled' OR [Status]='Released' OR [Status]='Active'))");
        });
        builder.HasKey(x => x.ReservationId);

        builder.Property(x => x.ReservationId)
            .HasColumnName("ReservationId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.ReservationId).UseIdentityColumn();
        builder.Property(x => x.LotId)
            .HasColumnName("LotId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.BinId)
            .HasColumnName("BinId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.SalesOrderDetailId)
            .HasColumnName("SalesOrderDetailId")
            .HasColumnType("bigint")
            .IsRequired(false);
        builder.Property(x => x.ReservedQuantity)
            .HasColumnName("ReservedQuantity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .IsRequired(true);
        builder.Property(x => x.Status)
            .HasColumnName("Status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasDefaultValueSql("('Active')")
            .IsRequired(true);
        builder.Property(x => x.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);

        builder.HasOne<WarehouseBin>()
            .WithMany()
            .HasForeignKey(x => x.BinId)
            .HasConstraintName("FK_StockReservations_Bins")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<InventoryLot>()
            .WithMany()
            .HasForeignKey(x => x.LotId)
            .HasConstraintName("FK_StockReservations_Lots")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
