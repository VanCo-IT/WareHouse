using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_SalesOrders_Status", "(([Status]='Cancelled' OR [Status]='Delivered' OR [Status]='PartiallyDelivered' OR [Status]='Confirmed' OR [Status]='Draft'))");
        });
        builder.HasKey(x => x.SalesOrderId);

        builder.Property(x => x.SalesOrderId)
            .HasColumnName("SalesOrderId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.SalesOrderId).UseIdentityColumn();
        builder.Property(x => x.SalesOrderNumber)
            .HasColumnName("SalesOrderNumber")
            .HasColumnType("varchar(40)")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.CustomerId)
            .HasColumnName("CustomerId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.OrderDate)
            .HasColumnName("OrderDate")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);
        builder.Property(x => x.CurrencyId)
            .HasColumnName("CurrencyId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ExchangeRate)
            .HasColumnName("ExchangeRate")
            .HasColumnType("decimal(18,6)")
            .HasPrecision(18, 6)
            .HasDefaultValueSql("((1))")
            .IsRequired(true);
        builder.Property(x => x.Status)
            .HasColumnName("Status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasDefaultValueSql("('Draft')")
            .IsRequired(true);
        builder.Property(x => x.CreatedByEmployeeId)
            .HasColumnName("CreatedByEmployeeId")
            .HasColumnType("int")
            .IsRequired(false);
        builder.Property(x => x.Note)
            .HasColumnName("Note")
            .HasColumnType("nvarchar(500)")
            .HasMaxLength(500)
            .IsUnicode(true)
            .IsRequired(false);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .HasConstraintName("FK_SalesOrders_Currencies")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .HasConstraintName("FK_SalesOrders_Customers")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.CreatedByEmployeeId)
            .HasConstraintName("FK_SalesOrders_Employees")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.SalesOrderNumber).IsUnique();
        builder.HasIndex(x => new { x.CustomerId, x.OrderDate }).HasDatabaseName("IX_SalesOrders_Customer_Date");
    }
}
