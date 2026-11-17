using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class WarehouseBinConfiguration : IEntityTypeConfiguration<WarehouseBin>
{
    public void Configure(EntityTypeBuilder<WarehouseBin> builder)
    {
        builder.ToTable("WarehouseBins");
        builder.HasKey(x => x.BinId);

        builder.Property(x => x.BinId)
            .HasColumnName("BinId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.BinId).UseIdentityColumn();
        builder.Property(x => x.ShelfId)
            .HasColumnName("ShelfId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.BinCode)
            .HasColumnName("BinCode")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.Capacity)
            .HasColumnName("Capacity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .IsRequired(false);
        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("bit")
            .HasDefaultValueSql("((1))")
            .HasSentinel(true)
            .IsRequired(true);

        builder.HasOne<WarehouseShelf>()
            .WithMany()
            .HasForeignKey(x => x.ShelfId)
            .HasConstraintName("FK_WarehouseBins_Shelves")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.ShelfId, x.BinCode }).IsUnique();
    }
}
