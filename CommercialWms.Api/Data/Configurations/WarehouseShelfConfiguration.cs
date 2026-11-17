using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class WarehouseShelfConfiguration : IEntityTypeConfiguration<WarehouseShelf>
{
    public void Configure(EntityTypeBuilder<WarehouseShelf> builder)
    {
        builder.ToTable("WarehouseShelves");
        builder.HasKey(x => x.ShelfId);

        builder.Property(x => x.ShelfId)
            .HasColumnName("ShelfId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ShelfId).UseIdentityColumn();
        builder.Property(x => x.RackId)
            .HasColumnName("RackId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ShelfCode)
            .HasColumnName("ShelfCode")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(true);

        builder.HasOne<WarehouseRack>()
            .WithMany()
            .HasForeignKey(x => x.RackId)
            .HasConstraintName("FK_WarehouseShelves_Racks")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.RackId, x.ShelfCode }).IsUnique();
    }
}
