using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class WarehouseTemperatureLogConfiguration : IEntityTypeConfiguration<WarehouseTemperatureLog>
{
    public void Configure(EntityTypeBuilder<WarehouseTemperatureLog> builder)
    {
        builder.ToTable("WarehouseTemperatureLogs");
        builder.HasKey(x => x.TemperatureLogId);

        builder.Property(x => x.TemperatureLogId)
            .HasColumnName("TemperatureLogId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.TemperatureLogId).UseIdentityColumn();
        builder.Property(x => x.WarehouseId)
            .HasColumnName("WarehouseId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.RecordedAt)
            .HasColumnName("RecordedAt")
            .HasColumnType("datetime2(7)")
            .IsRequired(true);
        builder.Property(x => x.TemperatureC)
            .HasColumnName("TemperatureC")
            .HasColumnType("decimal(5,2)")
            .HasPrecision(5, 2)
            .IsRequired(true);
        builder.Property(x => x.RecordedByEmployeeId)
            .HasColumnName("RecordedByEmployeeId")
            .HasColumnType("int")
            .IsRequired(false);
        builder.Property(x => x.Note)
            .HasColumnName("Note")
            .HasColumnType("nvarchar(300)")
            .HasMaxLength(300)
            .IsUnicode(true)
            .IsRequired(false);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .HasConstraintName("FK_WarehouseTemperatureLogs_Warehouses")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
