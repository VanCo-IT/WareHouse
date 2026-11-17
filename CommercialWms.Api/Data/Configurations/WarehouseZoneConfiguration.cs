using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class WarehouseZoneConfiguration : IEntityTypeConfiguration<WarehouseZone>
{
    public void Configure(EntityTypeBuilder<WarehouseZone> builder)
    {
        builder.ToTable("WarehouseZones");
        builder.HasKey(x => x.ZoneId);

        builder.Property(x => x.ZoneId)
            .HasColumnName("ZoneId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ZoneId).UseIdentityColumn();
        builder.Property(x => x.WarehouseId)
            .HasColumnName("WarehouseId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ZoneCode)
            .HasColumnName("ZoneCode")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.ZoneName)
            .HasColumnName("ZoneName")
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
            .IsUnicode(true)
            .IsRequired(true);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .HasConstraintName("FK_WarehouseZones_Warehouses")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.WarehouseId, x.ZoneCode }).IsUnique();
    }
}
