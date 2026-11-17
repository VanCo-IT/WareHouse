using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class WarehouseAisleConfiguration : IEntityTypeConfiguration<WarehouseAisle>
{
    public void Configure(EntityTypeBuilder<WarehouseAisle> builder)
    {
        builder.ToTable("WarehouseAisles");
        builder.HasKey(x => x.AisleId);

        builder.Property(x => x.AisleId)
            .HasColumnName("AisleId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.AisleId).UseIdentityColumn();
        builder.Property(x => x.ZoneId)
            .HasColumnName("ZoneId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.AisleCode)
            .HasColumnName("AisleCode")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(true);

        builder.HasOne<WarehouseZone>()
            .WithMany()
            .HasForeignKey(x => x.ZoneId)
            .HasConstraintName("FK_WarehouseAisles_Zones")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.ZoneId, x.AisleCode }).IsUnique();
    }
}
