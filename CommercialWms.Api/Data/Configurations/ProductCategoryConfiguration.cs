using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");
        builder.HasKey(x => x.CategoryId);

        builder.Property(x => x.CategoryId)
            .HasColumnName("CategoryId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.CategoryId).UseIdentityColumn();
        builder.Property(x => x.ParentCategoryId)
            .HasColumnName("ParentCategoryId")
            .HasColumnType("int")
            .IsRequired(false);
        builder.Property(x => x.CategoryCode)
            .HasColumnName("CategoryCode")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.CategoryName)
            .HasColumnName("CategoryName")
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("bit")
            .HasDefaultValueSql("((1))")
            .HasSentinel(true)
            .IsRequired(true);

        builder.HasOne<ProductCategory>()
            .WithMany()
            .HasForeignKey(x => x.ParentCategoryId)
            .HasConstraintName("FK_ProductCategories_Parent")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.CategoryCode).IsUnique();
    }
}
