using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class FoodSafetyDocumentConfiguration : IEntityTypeConfiguration<FoodSafetyDocument>
{
    public void Configure(EntityTypeBuilder<FoodSafetyDocument> builder)
    {
        builder.ToTable("FoodSafetyDocuments", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_FoodSafetyDocuments_Dates", "(([ExpiryDate] IS NULL OR [IssueDate] IS NULL OR [ExpiryDate]>=[IssueDate]))");
            tableBuilder.HasCheckConstraint("CK_FoodSafetyDocuments_Owner", "((((case when [SupplierId] IS NULL then (0) else (1) end+case when [ManufacturerId] IS NULL then (0) else (1) end)+case when [LotId] IS NULL then (0) else (1) end)=(1)))");
        });
        builder.HasKey(x => x.FoodSafetyDocumentId);

        builder.Property(x => x.FoodSafetyDocumentId)
            .HasColumnName("FoodSafetyDocumentId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.FoodSafetyDocumentId).UseIdentityColumn();
        builder.Property(x => x.SupplierId)
            .HasColumnName("SupplierId")
            .HasColumnType("int")
            .IsRequired(false);
        builder.Property(x => x.ManufacturerId)
            .HasColumnName("ManufacturerId")
            .HasColumnType("int")
            .IsRequired(false);
        builder.Property(x => x.LotId)
            .HasColumnName("LotId")
            .HasColumnType("bigint")
            .IsRequired(false);
        builder.Property(x => x.DocumentType)
            .HasColumnName("DocumentType")
            .HasColumnType("nvarchar(120)")
            .HasMaxLength(120)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.DocumentNumber)
            .HasColumnName("DocumentNumber")
            .HasColumnType("varchar(80)")
            .HasMaxLength(80)
            .IsUnicode(false)
            .IsRequired(false);
        builder.Property(x => x.IssueDate)
            .HasColumnName("IssueDate")
            .HasColumnType("date")
            .IsRequired(false);
        builder.Property(x => x.ExpiryDate)
            .HasColumnName("ExpiryDate")
            .HasColumnType("date")
            .IsRequired(false);
        builder.Property(x => x.FilePath)
            .HasColumnName("FilePath")
            .HasColumnType("nvarchar(500)")
            .HasMaxLength(500)
            .IsUnicode(true)
            .IsRequired(false);

        builder.HasOne<Manufacturer>()
            .WithMany()
            .HasForeignKey(x => x.ManufacturerId)
            .HasConstraintName("FK_FoodSafetyDocuments_Manufacturers")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .HasConstraintName("FK_FoodSafetyDocuments_Suppliers")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
