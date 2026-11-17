using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable("Deliveries", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Deliveries_Status", "(([Status]='Cancelled' OR [Status]='Failed' OR [Status]='Delivered' OR [Status]='Shipped' OR [Status]='Picking' OR [Status]='Draft'))");
        });
        builder.HasKey(x => x.DeliveryId);

        builder.Property(x => x.DeliveryId)
            .HasColumnName("DeliveryId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.DeliveryId).UseIdentityColumn();
        builder.Property(x => x.DeliveryNumber)
            .HasColumnName("DeliveryNumber")
            .HasColumnType("varchar(40)")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.SalesOrderId)
            .HasColumnName("SalesOrderId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.WarehouseId)
            .HasColumnName("WarehouseId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.DeliveryAddressId)
            .HasColumnName("DeliveryAddressId")
            .HasColumnType("int")
            .IsRequired(false);
        builder.Property(x => x.PlannedAt)
            .HasColumnName("PlannedAt")
            .HasColumnType("datetime2(7)")
            .IsRequired(false);
        builder.Property(x => x.ShippedAt)
            .HasColumnName("ShippedAt")
            .HasColumnType("datetime2(7)")
            .IsRequired(false);
        builder.Property(x => x.Status)
            .HasColumnName("Status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasDefaultValueSql("('Draft')")
            .IsRequired(true);

        builder.HasOne<CustomerAddress>()
            .WithMany()
            .HasForeignKey(x => x.DeliveryAddressId)
            .HasConstraintName("FK_Deliveries_CustomerAddresses")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<SalesOrder>()
            .WithMany()
            .HasForeignKey(x => x.SalesOrderId)
            .HasConstraintName("FK_Deliveries_SalesOrders")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .HasConstraintName("FK_Deliveries_Warehouses")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.DeliveryNumber).IsUnique();
    }
}
