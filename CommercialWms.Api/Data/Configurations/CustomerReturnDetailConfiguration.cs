using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class CustomerReturnDetailConfiguration : IEntityTypeConfiguration<CustomerReturnDetail>
{
    public void Configure(EntityTypeBuilder<CustomerReturnDetail> builder)
    {
        builder.ToTable("CustomerReturnDetails");
        builder.HasKey(x => x.CustomerReturnDetailId);

        builder.Property(x => x.CustomerReturnDetailId)
            .HasColumnName("CustomerReturnDetailId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.CustomerReturnDetailId).UseIdentityColumn();
        builder.Property(x => x.CustomerReturnId)
            .HasColumnName("CustomerReturnId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.ProductId)
            .HasColumnName("ProductId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.LotId)
            .HasColumnName("LotId")
            .HasColumnType("bigint")
            .IsRequired(false);
        builder.Property(x => x.Quantity)
            .HasColumnName("Quantity")
            .HasColumnType("decimal(18,3)")
            .HasPrecision(18, 3)
            .IsRequired(true);
        builder.Property(x => x.ConditionNote)
            .HasColumnName("ConditionNote")
            .HasColumnType("nvarchar(250)")
            .HasMaxLength(250)
            .IsUnicode(true)
            .IsRequired(false);

        builder.HasOne<InventoryLot>()
            .WithMany()
            .HasForeignKey(x => x.LotId)
            .HasConstraintName("FK_CustomerReturnDetails_Lots")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .HasConstraintName("FK_CustomerReturnDetails_Products")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<CustomerReturn>()
            .WithMany()
            .HasForeignKey(x => x.CustomerReturnId)
            .HasConstraintName("FK_CustomerReturnDetails_Returns")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
