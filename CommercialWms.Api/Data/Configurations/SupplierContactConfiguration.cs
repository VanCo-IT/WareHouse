using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class SupplierContactConfiguration : IEntityTypeConfiguration<SupplierContact>
{
    public void Configure(EntityTypeBuilder<SupplierContact> builder)
    {
        builder.ToTable("SupplierContacts");
        builder.HasKey(x => x.ContactId);

        builder.Property(x => x.ContactId)
            .HasColumnName("ContactId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ContactId).UseIdentityColumn();
        builder.Property(x => x.SupplierId)
            .HasColumnName("SupplierId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.ContactName)
            .HasColumnName("ContactName")
            .HasColumnType("nvarchar(120)")
            .HasMaxLength(120)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.JobTitle)
            .HasColumnName("JobTitle")
            .HasColumnType("nvarchar(80)")
            .HasMaxLength(80)
            .IsUnicode(true)
            .IsRequired(false);
        builder.Property(x => x.Email)
            .HasColumnName("Email")
            .HasColumnType("varchar(150)")
            .HasMaxLength(150)
            .IsUnicode(false)
            .IsRequired(false);
        builder.Property(x => x.Phone)
            .HasColumnName("Phone")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(false);

        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .HasConstraintName("FK_SupplierContacts_Suppliers")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
