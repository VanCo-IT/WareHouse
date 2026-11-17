using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("CustomerAddresses");
        builder.HasKey(x => x.AddressId);

        builder.Property(x => x.AddressId)
            .HasColumnName("AddressId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.AddressId).UseIdentityColumn();
        builder.Property(x => x.CustomerId)
            .HasColumnName("CustomerId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.AddressLine)
            .HasColumnName("AddressLine")
            .HasColumnType("nvarchar(300)")
            .HasMaxLength(300)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.Province)
            .HasColumnName("Province")
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
            .IsUnicode(true)
            .IsRequired(false);
        builder.Property(x => x.District)
            .HasColumnName("District")
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
            .IsUnicode(true)
            .IsRequired(false);
        builder.Property(x => x.IsDefault)
            .HasColumnName("IsDefault")
            .HasColumnType("bit")
            .HasDefaultValueSql("((0))")
            .HasSentinel(false)
            .IsRequired(true);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .HasConstraintName("FK_CustomerAddresses_Customers")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
