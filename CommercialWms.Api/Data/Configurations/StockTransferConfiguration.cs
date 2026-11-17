using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable("StockTransfers", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_StockTransfers_DifferentWarehouses", "(([SourceWarehouseId]<>[DestinationWarehouseId]))");
            tableBuilder.HasCheckConstraint("CK_StockTransfers_Status", "(([Status]='Cancelled' OR [Status]='Completed' OR [Status]='PartiallyReceived' OR [Status]='InTransit' OR [Status]='Approved' OR [Status]='Draft'))");
        });
        builder.HasKey(x => x.TransferId);

        builder.Property(x => x.TransferId)
            .HasColumnName("TransferId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.TransferId).UseIdentityColumn();
        builder.Property(x => x.TransferNumber)
            .HasColumnName("TransferNumber")
            .HasColumnType("varchar(40)")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.SourceWarehouseId)
            .HasColumnName("SourceWarehouseId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.DestinationWarehouseId)
            .HasColumnName("DestinationWarehouseId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.RequestedAt)
            .HasColumnName("RequestedAt")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);
        builder.Property(x => x.Status)
            .HasColumnName("Status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasDefaultValueSql("('Draft')")
            .IsRequired(true);
        builder.Property(x => x.RequestedByEmployeeId)
            .HasColumnName("RequestedByEmployeeId")
            .HasColumnType("int")
            .IsRequired(false);
        builder.Property(x => x.Note)
            .HasColumnName("Note")
            .HasColumnType("nvarchar(500)")
            .HasMaxLength(500)
            .IsUnicode(true)
            .IsRequired(false);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(x => x.DestinationWarehouseId)
            .HasConstraintName("FK_StockTransfers_DestinationWarehouse")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.RequestedByEmployeeId)
            .HasConstraintName("FK_StockTransfers_Employees")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(x => x.SourceWarehouseId)
            .HasConstraintName("FK_StockTransfers_SourceWarehouse")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.TransferNumber).IsUnique();
        builder.HasIndex(x => new { x.Status, x.RequestedAt }).HasDatabaseName("IX_StockTransfers_Status_Date");
    }
}
