using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class GoodsReceiptConfiguration : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("GoodsReceipts", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_GoodsReceipts_Status", "(([Status]='Cancelled' OR [Status]='Rejected' OR [Status]='PartiallyAccepted' OR [Status]='Accepted' OR [Status]='Checking' OR [Status]='Draft'))");
        });
        builder.HasKey(x => x.GoodsReceiptId);

        builder.Property(x => x.GoodsReceiptId)
            .HasColumnName("GoodsReceiptId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.GoodsReceiptId).UseIdentityColumn();
        builder.Property(x => x.ReceiptNumber)
            .HasColumnName("ReceiptNumber")
            .HasColumnType("varchar(40)")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.PurchaseOrderId)
            .HasColumnName("PurchaseOrderId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.WarehouseId)
            .HasColumnName("WarehouseId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ReceivedAt)
            .HasColumnName("ReceivedAt")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);
        builder.Property(x => x.ReceivedByEmployeeId)
            .HasColumnName("ReceivedByEmployeeId")
            .HasColumnType("int")
            .IsRequired(false);
        builder.Property(x => x.Status)
            .HasColumnName("Status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasDefaultValueSql("('Draft')")
            .IsRequired(true);
        builder.Property(x => x.Note)
            .HasColumnName("Note")
            .HasColumnType("nvarchar(500)")
            .HasMaxLength(500)
            .IsUnicode(true)
            .IsRequired(false);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.ReceivedByEmployeeId)
            .HasConstraintName("FK_GoodsReceipts_Employees")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<PurchaseOrder>()
            .WithMany()
            .HasForeignKey(x => x.PurchaseOrderId)
            .HasConstraintName("FK_GoodsReceipts_PurchaseOrders")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .HasConstraintName("FK_GoodsReceipts_Warehouses")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.ReceiptNumber).IsUnique();
        builder.HasIndex(x => new { x.WarehouseId, x.ReceivedAt }).HasDatabaseName("IX_GoodsReceipts_Warehouse_Date");
    }
}
