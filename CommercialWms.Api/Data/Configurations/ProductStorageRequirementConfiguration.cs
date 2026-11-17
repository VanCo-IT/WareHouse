using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class ProductStorageRequirementConfiguration : IEntityTypeConfiguration<ProductStorageRequirement>
{
    public void Configure(EntityTypeBuilder<ProductStorageRequirement> builder)
    {
        builder.ToTable("ProductStorageRequirements", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_ProductStorageRequirements_Dates", "(([EffectiveTo] IS NULL OR [EffectiveTo]>=[EffectiveFrom]))");
            tableBuilder.HasCheckConstraint("CK_ProductStorageRequirements_Temp", "(([MinTemperatureC] IS NULL OR [MaxTemperatureC] IS NULL OR [MinTemperatureC]<=[MaxTemperatureC]))");
        });
        builder.HasKey(x => x.RequirementId);

        builder.Property(x => x.RequirementId)
            .HasColumnName("RequirementId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.RequirementId).UseIdentityColumn();
        builder.Property(x => x.ProductId)
            .HasColumnName("ProductId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.MinTemperatureC)
            .HasColumnName("MinTemperatureC")
            .HasColumnType("decimal(5,2)")
            .HasPrecision(5, 2)
            .IsRequired(false);
        builder.Property(x => x.MaxTemperatureC)
            .HasColumnName("MaxTemperatureC")
            .HasColumnType("decimal(5,2)")
            .HasPrecision(5, 2)
            .IsRequired(false);
        builder.Property(x => x.StorageNote)
            .HasColumnName("StorageNote")
            .HasColumnType("nvarchar(300)")
            .HasMaxLength(300)
            .IsUnicode(true)
            .IsRequired(false);
        builder.Property(x => x.EffectiveFrom)
            .HasColumnName("EffectiveFrom")
            .HasColumnType("date")
            .IsRequired(true);
        builder.Property(x => x.EffectiveTo)
            .HasColumnName("EffectiveTo")
            .HasColumnType("date")
            .IsRequired(false);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .HasConstraintName("FK_ProductStorageRequirements_Products")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
