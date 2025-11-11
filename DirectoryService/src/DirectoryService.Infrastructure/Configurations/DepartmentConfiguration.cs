using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id).HasName("pk_department");

        builder.Property(d => d.Id)
            .IsRequired()
            .HasConversion(d => d.Value, d => new DepartmentId(d))
            .HasColumnName("department_id");

        builder
            .Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(LengthConstants.LENGTH150)
            .HasColumnName("department_name")
            .HasConversion(d => d.Value, d => DepartmentName.Create(d).Value);

        builder.Property(d => d.Identifier)
            .IsRequired()
            .HasMaxLength(LengthConstants.LENGTH150)
            .HasColumnName("identifier");

        builder.Property(d => d.ParentId)
            .IsRequired(false)
            .HasColumnName("parent_id")
            .HasConversion(
                d => d == null ? (Guid?)null : d.Value,
                d => d.HasValue ? new DepartmentId(d.Value) : null);

        builder.HasOne(d => d.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(d => d.Children)
            .HasField("_children")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(d => d.Path)
            .HasColumnName("path");

        builder.Property(d => d.Depth)
            .HasColumnName("depth");

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(d => d.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.Navigation(d => d.DepartmentLocations)
            .HasField("_locations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(d => d.DepartmentPositions)
            .WithOne(dp => dp.Department)
            .HasForeignKey(dp => dp.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.DepartmentPositions)
            .HasField("_positions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}