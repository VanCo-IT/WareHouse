using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class PaymentDetailConfiguration : IEntityTypeConfiguration<PaymentDetail>
{
    public void Configure(EntityTypeBuilder<PaymentDetail> builder)
    {
        builder.ToTable("PaymentDetails");
        builder.HasKey(x => x.PaymentDetailId);

        builder.Property(x => x.PaymentDetailId)
            .HasColumnName("PaymentDetailId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.PaymentDetailId).UseIdentityColumn();
        builder.Property(x => x.PaymentId)
            .HasColumnName("PaymentId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.SalesOrderId)
            .HasColumnName("SalesOrderId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.AppliedAmount)
            .HasColumnName("AppliedAmount")
            .HasColumnType("decimal(18,4)")
            .HasPrecision(18, 4)
            .IsRequired(true);

        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .HasConstraintName("FK_PaymentDetails_Payments")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<SalesOrder>()
            .WithMany()
            .HasForeignKey(x => x.SalesOrderId)
            .HasConstraintName("FK_PaymentDetails_SalesOrders")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.PaymentId, x.SalesOrderId }).IsUnique();
    }
}
