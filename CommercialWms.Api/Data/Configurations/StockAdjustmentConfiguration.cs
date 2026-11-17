using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ToTable("StockAdjustments", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_StockAdjustments_Status", "(([Status]='Cancelled' OR [Status]='Rejected' OR [Status]='Approved' OR [Status]='Draft'))");
        });
        builder.HasKey(x => x.AdjustmentId);

        builder.Property(x => x.AdjustmentId)
            .HasColumnName("AdjustmentId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.AdjustmentId).UseIdentityColumn();
        builder.Property(x => x.AdjustmentNumber)
            .HasColumnName("AdjustmentNumber")
            .HasColumnType("varchar(40)")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.WarehouseId)
            .HasColumnName("WarehouseId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.AdjustmentDate)
            .HasColumnName("AdjustmentDate")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);
        builder.Property(x => x.Reason)
            .HasColumnName("Reason")
            .HasColumnType("nvarchar(250)")
            .HasMaxLength(250)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.Status)
            .HasColumnName("Status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasDefaultValueSql("('Draft')")
            .IsRequired(true);
        builder.Property(x => x.CreatedByEmployeeId)
            .HasColumnName("CreatedByEmployeeId")
            .HasColumnType("int")
            .IsRequired(false);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.CreatedByEmployeeId)
            .HasConstraintName("FK_StockAdjustments_Employees")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .HasConstraintName("FK_StockAdjustments_Warehouses")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.AdjustmentNumber).IsUnique();
    }
}
