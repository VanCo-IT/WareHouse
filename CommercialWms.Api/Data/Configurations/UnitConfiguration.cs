using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Units");
        builder.HasKey(x => x.UnitId);

        builder.Property(x => x.UnitId)
            .HasColumnName("UnitId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.UnitId).UseIdentityColumn();
        builder.Property(x => x.UnitCode)
            .HasColumnName("UnitCode")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.UnitName)
            .HasColumnName("UnitName")
            .HasColumnType("nvarchar(60)")
            .HasMaxLength(60)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("bit")
            .HasDefaultValueSql("((1))")
            .HasSentinel(true)
            .IsRequired(true);


        builder.HasIndex(x => x.UnitCode).IsUnique();

        builder.HasData(new Unit { UnitId = 1, UnitCode = "EA", UnitName = "Each", IsActive = true });
    }
}
