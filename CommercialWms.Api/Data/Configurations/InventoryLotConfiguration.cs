using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class InventoryLotConfiguration : IEntityTypeConfiguration<InventoryLot>
{
    public void Configure(EntityTypeBuilder<InventoryLot> builder)
    {
        builder.ToTable("InventoryLots", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_InventoryLots_Dates", "(([ExpiryDate] IS NULL OR [ManufactureDate] IS NULL OR [ExpiryDate]>=[ManufactureDate]))");
        });
        builder.HasKey(x => x.LotId);

        builder.Property(x => x.LotId)
            .HasColumnName("LotId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.LotId).UseIdentityColumn();
        builder.Property(x => x.LotCode)
            .HasColumnName("LotCode")
            .HasColumnType("varchar(60)")
            .HasMaxLength(60)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.ProductId)
            .HasColumnName("ProductId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ManufacturerId)
            .HasColumnName("ManufacturerId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.CountryId)
            .HasColumnName("CountryId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ManufactureDate)
            .HasColumnName("ManufactureDate")
            .HasColumnType("date")
            .IsRequired(false);
        builder.Property(x => x.ExpiryDate)
            .HasColumnName("ExpiryDate")
            .HasColumnType("date")
            .IsRequired(false);
        builder.Property(x => x.SupplierLotCode)
            .HasColumnName("SupplierLotCode")
            .HasColumnType("varchar(60)")
            .HasMaxLength(60)
            .IsUnicode(false)
            .IsRequired(false);
        builder.Property(x => x.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .HasConstraintName("FK_InventoryLots_Countries")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Manufacturer>()
            .WithMany()
            .HasForeignKey(x => x.ManufacturerId)
            .HasConstraintName("FK_InventoryLots_Manufacturers")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .HasConstraintName("FK_InventoryLots_Products")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.ProductId, x.LotCode }).IsUnique();
        builder.HasIndex(x => new { x.ProductId, x.ExpiryDate }).HasDatabaseName("IX_InventoryLots_Product_Expiry");
    }
}
