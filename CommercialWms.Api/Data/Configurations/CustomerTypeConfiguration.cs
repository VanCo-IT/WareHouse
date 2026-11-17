using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class CustomerTypeConfiguration : IEntityTypeConfiguration<CustomerType>
{
    public void Configure(EntityTypeBuilder<CustomerType> builder)
    {
        builder.ToTable("CustomerTypes");
        builder.HasKey(x => x.CustomerTypeId);

        builder.Property(x => x.CustomerTypeId)
            .HasColumnName("CustomerTypeId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.CustomerTypeId).UseIdentityColumn();
        builder.Property(x => x.TypeCode)
            .HasColumnName("TypeCode")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.TypeName)
            .HasColumnName("TypeName")
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
            .IsUnicode(true)
            .IsRequired(true);


        builder.HasIndex(x => x.TypeCode).IsUnique();

        builder.HasData(new CustomerType { CustomerTypeId = 1, TypeCode = "RETAIL", TypeName = "Retail" });
    }
}
