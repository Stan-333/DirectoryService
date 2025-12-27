using DirectoryService.Domain.Departments;
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
            .HasMaxLength(DepartmentName.NAME_MAX_LENGTH)
            .HasColumnName("department_name")
            .HasConversion(d => d.Value, s => DepartmentName.Create(s).Value);

        builder.Property(d => d.Identifier)
            .IsRequired()
            .HasMaxLength(Identifier.MAX_LENGTH)
            .HasColumnName("identifier")
            .HasConversion(i => i.Value, s => Identifier.Create(s).Value);

        builder.HasIndex(d => d.Identifier)
            .HasDatabaseName("idx_department_identifier")
            .HasFilter("is_active = true")
            .IsUnique();

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

        builder.Ignore(d => d.ChildrenCount);

        builder.Property(d => d.Path)
            .HasColumnName("path")
            .HasColumnType("ltree");

        builder.HasIndex(d => d.Path)
            .HasMethod("gist")
            .HasDatabaseName("idx_department_path");

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