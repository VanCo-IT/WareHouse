using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class ManufacturerDocumentConfiguration : IEntityTypeConfiguration<ManufacturerDocument>
{
    public void Configure(EntityTypeBuilder<ManufacturerDocument> builder)
    {
        builder.ToTable("ManufacturerDocuments", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_ManufacturerDocuments_Dates", "(([ExpiryDate] IS NULL OR [IssueDate] IS NULL OR [ExpiryDate]>=[IssueDate]))");
        });
        builder.HasKey(x => x.DocumentId);

        builder.Property(x => x.DocumentId)
            .HasColumnName("DocumentId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.DocumentId).UseIdentityColumn();
        builder.Property(x => x.ManufacturerId)
            .HasColumnName("ManufacturerId")
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

        builder.HasOne<Manufacturer>()
            .WithMany()
            .HasForeignKey(x => x.ManufacturerId)
            .HasConstraintName("FK_ManufacturerDocuments_Manufacturers")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
