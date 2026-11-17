using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class QualityInspectionDetailConfiguration : IEntityTypeConfiguration<QualityInspectionDetail>
{
    public void Configure(EntityTypeBuilder<QualityInspectionDetail> builder)
    {
        builder.ToTable("QualityInspectionDetails", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_QualityInspectionDetails_Result", "(([Result]='NotApplicable' OR [Result]='Pending' OR [Result]='Fail' OR [Result]='Pass'))");
        });
        builder.HasKey(x => x.QualityInspectionDetailId);

        builder.Property(x => x.QualityInspectionDetailId)
            .HasColumnName("QualityInspectionDetailId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.QualityInspectionDetailId).UseIdentityColumn();
        builder.Property(x => x.QualityInspectionId)
            .HasColumnName("QualityInspectionId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.GoodsReceiptDetailId)
            .HasColumnName("GoodsReceiptDetailId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.CheckName)
            .HasColumnName("CheckName")
            .HasColumnType("nvarchar(120)")
            .HasMaxLength(120)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.Result)
            .HasColumnName("Result")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.Note)
            .HasColumnName("Note")
            .HasColumnType("nvarchar(300)")
            .HasMaxLength(300)
            .IsUnicode(true)
            .IsRequired(false);

        builder.HasOne<QualityInspection>()
            .WithMany()
            .HasForeignKey(x => x.QualityInspectionId)
            .HasConstraintName("FK_QualityInspectionDetails_Inspections")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<GoodsReceiptDetail>()
            .WithMany()
            .HasForeignKey(x => x.GoodsReceiptDetailId)
            .HasConstraintName("FK_QualityInspectionDetails_ReceiptDetails")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
