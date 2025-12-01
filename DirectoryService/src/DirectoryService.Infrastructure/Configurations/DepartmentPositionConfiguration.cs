using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");

        builder.HasKey(dp => new { dp.DepartmentId, dp.PositionId })
            .HasName("pk_department_position");

        builder.Property(dp => dp.DepartmentId)
            .HasConversion(dp => dp.Value, dp => new DepartmentId(dp))
            .HasColumnName("department_id");

        builder.Property(dp => dp.PositionId)
            .HasConversion(dp => dp.Value, dp => new PositionId(dp))
            .HasColumnName("position_id");

        builder.HasOne(dp => dp.Department)
            .WithMany(d => d.DepartmentPositions)
            .HasForeignKey(dp => dp.DepartmentId)
            .HasConstraintName("fk_department_positions_department");

        builder.HasOne(dp => dp.Position)
            .WithMany(p => p.DepartmentPositions)
            .HasForeignKey(dp => dp.PositionId)
            .HasConstraintName("fk_department_positions_position");
    }
}