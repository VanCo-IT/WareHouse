using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class CustomerReturnConfiguration : IEntityTypeConfiguration<CustomerReturn>
{
    public void Configure(EntityTypeBuilder<CustomerReturn> builder)
    {
        builder.ToTable("CustomerReturns", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_CustomerReturns_Status", "(([Status]='Completed' OR [Status]='Rejected' OR [Status]='Accepted' OR [Status]='Pending'))");
        });
        builder.HasKey(x => x.CustomerReturnId);

        builder.Property(x => x.CustomerReturnId)
            .HasColumnName("CustomerReturnId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.CustomerReturnId).UseIdentityColumn();
        builder.Property(x => x.ReturnNumber)
            .HasColumnName("ReturnNumber")
            .HasColumnType("varchar(40)")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.CustomerId)
            .HasColumnName("CustomerId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.SalesOrderId)
            .HasColumnName("SalesOrderId")
            .HasColumnType("bigint")
            .IsRequired(false);
        builder.Property(x => x.ReturnedAt)
            .HasColumnName("ReturnedAt")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);
        builder.Property(x => x.Reason)
            .HasColumnName("Reason")
            .HasColumnType("nvarchar(300)")
            .HasMaxLength(300)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.Status)
            .HasColumnName("Status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasDefaultValueSql("('Pending')")
            .IsRequired(true);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .HasConstraintName("FK_CustomerReturns_Customers")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<SalesOrder>()
            .WithMany()
            .HasForeignKey(x => x.SalesOrderId)
            .HasConstraintName("FK_CustomerReturns_SalesOrders")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.ReturnNumber).IsUnique();
    }
}
