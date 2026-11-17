using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class StockCountConfiguration : IEntityTypeConfiguration<StockCount>
{
    public void Configure(EntityTypeBuilder<StockCount> builder)
    {
        builder.ToTable("StockCounts", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_StockCounts_Status", "(([Status]='Cancelled' OR [Status]='Completed' OR [Status]='Reviewed' OR [Status]='Counting' OR [Status]='Draft'))");
        });
        builder.HasKey(x => x.StockCountId);

        builder.Property(x => x.StockCountId)
            .HasColumnName("StockCountId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.StockCountId).UseIdentityColumn();
        builder.Property(x => x.StockCountNumber)
            .HasColumnName("StockCountNumber")
            .HasColumnType("varchar(40)")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.WarehouseId)
            .HasColumnName("WarehouseId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.CountDate)
            .HasColumnName("CountDate")
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
        builder.Property(x => x.CreatedByEmployeeId)
            .HasColumnName("CreatedByEmployeeId")
            .HasColumnType("int")
            .IsRequired(false);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.CreatedByEmployeeId)
            .HasConstraintName("FK_StockCounts_Employees")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .HasConstraintName("FK_StockCounts_Warehouses")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.StockCountNumber).IsUnique();
    }
}
