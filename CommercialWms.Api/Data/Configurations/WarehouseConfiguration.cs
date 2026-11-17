using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(x => x.WarehouseId);

        builder.Property(x => x.WarehouseId)
            .HasColumnName("WarehouseId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.WarehouseId).UseIdentityColumn();
        builder.Property(x => x.WarehouseCode)
            .HasColumnName("WarehouseCode")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.WarehouseName)
            .HasColumnName("WarehouseName")
            .HasColumnType("nvarchar(150)")
            .HasMaxLength(150)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.Address)
            .HasColumnName("Address")
            .HasColumnType("nvarchar(300)")
            .HasMaxLength(300)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("bit")
            .HasDefaultValueSql("((1))")
            .HasSentinel(true)
            .IsRequired(true);


        builder.HasIndex(x => x.WarehouseCode).IsUnique();
    }
}
