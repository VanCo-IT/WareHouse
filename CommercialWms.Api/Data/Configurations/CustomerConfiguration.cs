using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(x => x.CustomerId);

        builder.Property(x => x.CustomerId)
            .HasColumnName("CustomerId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.CustomerId).UseIdentityColumn();
        builder.Property(x => x.CustomerCode)
            .HasColumnName("CustomerCode")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.CustomerName)
            .HasColumnName("CustomerName")
            .HasColumnType("nvarchar(180)")
            .HasMaxLength(180)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.CustomerTypeId)
            .HasColumnName("CustomerTypeId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.TaxCode)
            .HasColumnName("TaxCode")
            .HasColumnType("varchar(40)")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired(false);
        builder.Property(x => x.Phone)
            .HasColumnName("Phone")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(false);
        builder.Property(x => x.Email)
            .HasColumnName("Email")
            .HasColumnType("varchar(150)")
            .HasMaxLength(150)
            .IsUnicode(false)
            .IsRequired(false);
        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("bit")
            .HasDefaultValueSql("((1))")
            .HasSentinel(true)
            .IsRequired(true);

        builder.HasOne<CustomerType>()
            .WithMany()
            .HasForeignKey(x => x.CustomerTypeId)
            .HasConstraintName("FK_Customers_CustomerTypes")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.CustomerCode).IsUnique();
    }
}
