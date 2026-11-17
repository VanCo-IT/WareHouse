using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class SalesOrderDetailConfiguration : IEntityTypeConfiguration<SalesOrderDetail>
{
    public void Configure(EntityTypeBuilder<SalesOrderDetail> builder)
    {
        builder.ToTable("SalesOrderDetails");
        builder.HasKey(x => x.SalesOrderDetailId);

        builder.Property(x => x.SalesOrderDetailId)
            .HasColumnName("SalesOrderDetailId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.SalesOrderDetailId).UseIdentityColumn();
        builder.Property(x => x.SalesOrderId)
            .HasColumnName("SalesOrderId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.ProductId)
            .HasColumnName("ProductId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.Quantity)
            .HasColumnName("Quantity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .IsRequired(true);
        builder.Property(x => x.UnitId)
            .HasColumnName("UnitId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.UnitPrice)
            .HasColumnName("UnitPrice")
            .HasColumnType("decimal(18,4)")
            .HasPrecision(18, 4)
            .IsRequired(true);
        builder.Property(x => x.DiscountAmount)
            .HasColumnName("DiscountAmount")
            .HasColumnType("decimal(18,4)")
            .HasPrecision(18, 4)
            .HasDefaultValueSql("((0))")
            .IsRequired(true);

        builder.HasOne<SalesOrder>()
            .WithMany()
            .HasForeignKey(x => x.SalesOrderId)
            .HasConstraintName("FK_SalesOrderDetails_Orders")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .HasConstraintName("FK_SalesOrderDetails_Products")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Unit>()
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_SalesOrderDetails_Units")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
