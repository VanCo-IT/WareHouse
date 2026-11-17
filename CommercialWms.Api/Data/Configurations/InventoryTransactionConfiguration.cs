using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_InventoryTransactions_Type", "(([TransactionType]='Return' OR [TransactionType]='Adjustment' OR [TransactionType]='TransferIn' OR [TransactionType]='TransferOut' OR [TransactionType]='Issue' OR [TransactionType]='Receipt'))");
        });
        builder.HasKey(x => x.TransactionId);

        builder.Property(x => x.TransactionId)
            .HasColumnName("TransactionId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.TransactionId).UseIdentityColumn();
        builder.Property(x => x.LotId)
            .HasColumnName("LotId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.WarehouseId)
            .HasColumnName("WarehouseId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.BinId)
            .HasColumnName("BinId")
            .HasColumnType("int")
            .IsRequired(false);
        builder.Property(x => x.TransactionType)
            .HasColumnName("TransactionType")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.QuantityChange)
            .HasColumnName("QuantityChange")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .IsRequired(true);
        builder.Property(x => x.ReferenceType)
            .HasColumnName("ReferenceType")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(false);
        builder.Property(x => x.ReferenceId)
            .HasColumnName("ReferenceId")
            .HasColumnType("bigint")
            .IsRequired(false);
        builder.Property(x => x.OccurredAt)
            .HasColumnName("OccurredAt")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);
        builder.Property(x => x.CreatedByEmployeeId)
            .HasColumnName("CreatedByEmployeeId")
            .HasColumnType("int")
            .IsRequired(false);
        builder.Property(x => x.Note)
            .HasColumnName("Note")
            .HasColumnType("nvarchar(300)")
            .HasMaxLength(300)
            .IsUnicode(true)
            .IsRequired(false);

        builder.HasOne<WarehouseBin>()
            .WithMany()
            .HasForeignKey(x => x.BinId)
            .HasConstraintName("FK_InventoryTransactions_Bins")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.CreatedByEmployeeId)
            .HasConstraintName("FK_InventoryTransactions_Employees")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<InventoryLot>()
            .WithMany()
            .HasForeignKey(x => x.LotId)
            .HasConstraintName("FK_InventoryTransactions_Lots")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .HasConstraintName("FK_InventoryTransactions_Warehouses")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.LotId, x.OccurredAt }).HasDatabaseName("IX_InventoryTransactions_Lot_Date");
    }
}
