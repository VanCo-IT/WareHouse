using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class InventoryBalanceConfiguration : IEntityTypeConfiguration<InventoryBalance>
{
    public void Configure(EntityTypeBuilder<InventoryBalance> builder)
    {
        builder.ToTable("InventoryBalances", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_InventoryBalances_Reserved", "(([QuantityReserved]<=[QuantityOnHand]))");
        });
        builder.HasKey(x => x.BalanceId);

        builder.Property(x => x.BalanceId)
            .HasColumnName("BalanceId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.BalanceId).UseIdentityColumn();
        builder.Property(x => x.LotId)
            .HasColumnName("LotId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.BinId)
            .HasColumnName("BinId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.QuantityOnHand)
            .HasColumnName("QuantityOnHand")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .HasDefaultValueSql("((0))")
            .IsRequired(true);
        builder.Property(x => x.QuantityReserved)
            .HasColumnName("QuantityReserved")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .HasDefaultValueSql("((0))")
            .IsRequired(true);
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);

        builder.HasOne<WarehouseBin>()
            .WithMany()
            .HasForeignKey(x => x.BinId)
            .HasConstraintName("FK_InventoryBalances_Bins")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<InventoryLot>()
            .WithMany()
            .HasForeignKey(x => x.LotId)
            .HasConstraintName("FK_InventoryBalances_Lots")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.LotId, x.BinId }).IsUnique();
        builder.HasIndex(x => x.BinId).HasDatabaseName("IX_InventoryBalances_BinId");
    }
}
