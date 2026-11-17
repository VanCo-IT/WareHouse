using CommercialWms.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialWms.Api.Data.Configurations;

public sealed class WarehouseEmployeeConfiguration : IEntityTypeConfiguration<WarehouseEmployee>
{
    public void Configure(EntityTypeBuilder<WarehouseEmployee> builder)
    {
        builder.ToTable("WarehouseEmployees", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_WarehouseEmployees_Dates", "(([AssignedTo] IS NULL OR [AssignedTo]>=[AssignedFrom]))");
        });
        builder.HasKey(x => x.WarehouseEmployeeId);

        builder.Property(x => x.WarehouseEmployeeId)
            .HasColumnName("WarehouseEmployeeId")
            .HasColumnType("bigint")
            .IsRequired(true);
        builder.Property(x => x.WarehouseEmployeeId).UseIdentityColumn();
        builder.Property(x => x.WarehouseId)
            .HasColumnName("WarehouseId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.EmployeeId)
            .HasColumnName("EmployeeId")
            .HasColumnType("int")
            .IsRequired(true);
        builder.Property(x => x.AssignedFrom)
            .HasColumnName("AssignedFrom")
            .HasColumnType("date")
            .IsRequired(true);
        builder.Property(x => x.AssignedTo)
            .HasColumnName("AssignedTo")
            .HasColumnType("date")
            .IsRequired(false);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .HasConstraintName("FK_WarehouseEmployees_Warehouses")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.WarehouseId, x.EmployeeId, x.AssignedFrom }).IsUnique();
    }
}
