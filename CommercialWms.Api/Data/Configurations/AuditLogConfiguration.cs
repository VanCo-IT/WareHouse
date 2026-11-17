using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.AuditLogId);

        builder.Property(x => x.AuditLogId)
            .HasColumnName("AuditLogId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.AuditLogId).UseIdentityColumn();
        builder.Property(x => x.EmployeeId)
            .HasColumnName("EmployeeId")
            .HasColumnType("int")
            .IsRequired(false);
        builder.Property(x => x.ActionName)
            .HasColumnName("ActionName")
            .HasColumnType("varchar(80)")
            .HasMaxLength(80)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.EntityName)
            .HasColumnName("EntityName")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.EntityId)
            .HasColumnName("EntityId")
            .HasColumnType("varchar(80)")
            .HasMaxLength(80)
            .IsUnicode(false)
            .IsRequired(false);
        builder.Property(x => x.OldValues)
            .HasColumnName("OldValues")
            .HasColumnType("nvarchar(max)")
            .IsUnicode(true)
            .IsRequired(false);
        builder.Property(x => x.NewValues)
            .HasColumnName("NewValues")
            .HasColumnType("nvarchar(max)")
            .IsUnicode(true)
            .IsRequired(false);
        builder.Property(x => x.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasColumnType("datetime2(7)")
            .HasDefaultValueSql("(sysutcdatetime())")
            .IsRequired(true);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .HasConstraintName("FK_AuditLogs_Employees")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
