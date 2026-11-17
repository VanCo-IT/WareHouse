using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class PurchasePriceHistoryConfiguration : IEntityTypeConfiguration<PurchasePriceHistory>
{
    public void Configure(EntityTypeBuilder<PurchasePriceHistory> builder)
    {
        builder.ToTable("PurchasePriceHistories");
        builder.HasKey(x => x.PurchasePriceHistoryId);

        builder.Property(x => x.PurchasePriceHistoryId)
            .HasColumnName("PurchasePriceHistoryId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.PurchasePriceHistoryId).UseIdentityColumn();
        builder.Property(x => x.ProductId)
            .HasColumnName("ProductId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.SupplierId)
            .HasColumnName("SupplierId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.PurchaseOrderDetailId)
            .HasColumnName("PurchaseOrderDetailId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.CurrencyId)
            .HasColumnName("CurrencyId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.UnitPrice)
            .HasColumnName("UnitPrice")
            .HasColumnType("decimal(18,4)")
            .HasPrecision(18, 4)
            .IsRequired(true);
        builder.Property(x => x.RecordedAt)
            .HasColumnName("RecordedAt")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .HasConstraintName("FK_PurchasePriceHistories_Currencies")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<PurchaseOrderDetail>()
            .WithMany()
            .HasForeignKey(x => x.PurchaseOrderDetailId)
            .HasConstraintName("FK_PurchasePriceHistories_PODetails")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .HasConstraintName("FK_PurchasePriceHistories_Products")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .HasConstraintName("FK_PurchasePriceHistories_Suppliers")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
