using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_PurchaseOrders_Status", "(([Status]='Cancelled' OR [Status]='Received' OR [Status]='PartiallyReceived' OR [Status]='Approved' OR [Status]='Submitted' OR [Status]='Draft'))");
        });
        builder.HasKey(x => x.PurchaseOrderId);

        builder.Property(x => x.PurchaseOrderId)
            .HasColumnName("PurchaseOrderId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.PurchaseOrderId).UseIdentityColumn();
        builder.Property(x => x.PONumber)
            .HasColumnName("PONumber")
            .HasColumnType("varchar(40)")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.SupplierId)
            .HasColumnName("SupplierId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.OrderDate)
            .HasColumnName("OrderDate")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);
        builder.Property(x => x.ExpectedDate)
            .HasColumnName("ExpectedDate")
            .HasColumnType("date")
            .IsRequired(false);
        builder.Property(x => x.CurrencyId)
            .HasColumnName("CurrencyId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ExchangeRate)
            .HasColumnName("ExchangeRate")
            .HasColumnType("decimal(18,6)")
            .HasPrecision(18, 6)
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
            .HasConstraintName("FK_PurchaseOrders_Currencies")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.CreatedByEmployeeId)
            .HasConstraintName("FK_PurchaseOrders_Employees")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .HasConstraintName("FK_PurchaseOrders_Suppliers")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.PONumber).IsUnique();
        builder.HasIndex(x => new { x.SupplierId, x.OrderDate }).HasDatabaseName("IX_PurchaseOrders_Supplier_Date");
    }
}
