using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class WarehouseRackConfiguration : IEntityTypeConfiguration<WarehouseRack>
{
    public void Configure(EntityTypeBuilder<WarehouseRack> builder)
    {
        builder.ToTable("WarehouseRacks");
        builder.HasKey(x => x.RackId);

        builder.Property(x => x.RackId)
            .HasColumnName("RackId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.RackId).UseIdentityColumn();
        builder.Property(x => x.AisleId)
            .HasColumnName("AisleId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.RackCode)
            .HasColumnName("RackCode")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(true);

        builder.HasOne<WarehouseAisle>()
            .WithMany()
            .HasForeignKey(x => x.AisleId)
            .HasConstraintName("FK_WarehouseRacks_Aisles")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.AisleId, x.RackCode }).IsUnique();
    }
}
