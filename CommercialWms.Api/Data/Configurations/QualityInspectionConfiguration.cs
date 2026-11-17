using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class QualityInspectionConfiguration : IEntityTypeConfiguration<QualityInspection>
{
    public void Configure(EntityTypeBuilder<QualityInspection> builder)
    {
        builder.ToTable("QualityInspections", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_QualityInspections_Result", "(([Result]='Conditional' OR [Result]='Failed' OR [Result]='Passed' OR [Result]='Pending'))");
        });
        builder.HasKey(x => x.QualityInspectionId);

        builder.Property(x => x.QualityInspectionId)
            .HasColumnName("QualityInspectionId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.QualityInspectionId).UseIdentityColumn();
        builder.Property(x => x.InspectionNumber)
            .HasColumnName("InspectionNumber")
            .HasColumnType("varchar(40)")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.GoodsReceiptId)
            .HasColumnName("GoodsReceiptId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.InspectedAt)
            .HasColumnName("InspectedAt")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);
        builder.Property(x => x.InspectorEmployeeId)
            .HasColumnName("InspectorEmployeeId")
            .HasColumnType("int")
            .IsRequired(false);
        builder.Property(x => x.Result)
            .HasColumnName("Result")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasDefaultValueSql("('Pending')")
            .IsRequired(true);
        builder.Property(x => x.Note)
            .HasColumnName("Note")
            .HasColumnType("nvarchar(500)")
            .HasMaxLength(500)
            .IsUnicode(true)
            .IsRequired(false);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.InspectorEmployeeId)
            .HasConstraintName("FK_QualityInspections_Employees")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<GoodsReceipt>()
            .WithMany()
            .HasForeignKey(x => x.GoodsReceiptId)
            .HasConstraintName("FK_QualityInspections_Receipts")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.InspectionNumber).IsUnique();
    }
}
