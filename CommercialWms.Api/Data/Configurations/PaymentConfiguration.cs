using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(x => x.PaymentId);

        builder.Property(x => x.PaymentId)
            .HasColumnName("PaymentId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.PaymentId).UseIdentityColumn();
        builder.Property(x => x.PaymentNumber)
            .HasColumnName("PaymentNumber")
            .HasColumnType("varchar(40)")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.CustomerId)
            .HasColumnName("CustomerId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.PaymentDate)
            .HasColumnName("PaymentDate")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);
        builder.Property(x => x.CurrencyId)
            .HasColumnName("CurrencyId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.Amount)
            .HasColumnName("Amount")
            .HasColumnType("decimal(18,4)")
            .HasPrecision(18, 4)
            .IsRequired(true);
        builder.Property(x => x.PaymentMethod)
            .HasColumnName("PaymentMethod")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.Status)
            .HasColumnName("Status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasDefaultValueSql("('Completed')")
            .IsRequired(true);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .HasConstraintName("FK_Payments_Currencies")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .HasConstraintName("FK_Payments_Customers")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.PaymentNumber).IsUnique();
    }
}
