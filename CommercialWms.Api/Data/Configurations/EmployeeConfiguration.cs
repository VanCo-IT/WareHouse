using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(x => x.EmployeeId);

        builder.Property(x => x.EmployeeId)
            .HasColumnName("EmployeeId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.EmployeeId).UseIdentityColumn();
        builder.Property(x => x.EmployeeCode)
            .HasColumnName("EmployeeCode")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(true);
        builder.Property(x => x.FullName)
            .HasColumnName("FullName")
            .HasColumnType("nvarchar(150)")
            .HasMaxLength(150)
            .IsUnicode(true)
            .IsRequired(true);
        builder.Property(x => x.Email)
            .HasColumnName("Email")
            .HasColumnType("varchar(150)")
            .HasMaxLength(150)
            .IsUnicode(false)
            .IsRequired(false);
        builder.Property(x => x.Phone)
            .HasColumnName("Phone")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired(false);
        builder.Property(x => x.JobTitle)
            .HasColumnName("JobTitle")
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
            .IsUnicode(true)
            .IsRequired(false);
        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("bit")
            .HasDefaultValueSql("((1))")
            .HasSentinel(true)
            .IsRequired(true);


        builder.HasIndex(x => x.EmployeeCode).IsUnique();
    }
}
