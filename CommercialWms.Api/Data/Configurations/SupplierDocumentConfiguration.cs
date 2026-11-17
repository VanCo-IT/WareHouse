using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class SupplierDocumentConfiguration : IEntityTypeConfiguration<SupplierDocument>
{
    public void Configure(EntityTypeBuilder<SupplierDocument> builder)
    {
        builder.ToTable("SupplierDocuments", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_SupplierDocuments_Dates", "(([ExpiryDate] IS NULL OR [IssueDate] IS NULL OR [ExpiryDate]>=[IssueDate]))");
        });
        builder.HasKey(x => x.DocumentId);

        builder.Property(x => x.DocumentId)
            .HasColumnName("DocumentId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.DocumentId).UseIdentityColumn();
        builder.Property(x => x.SupplierId)
            .HasColumnName("SupplierId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.DocumentType)
            .HasColumnName("DocumentType")
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
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

        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .HasConstraintName("FK_SupplierDocuments_Suppliers")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
