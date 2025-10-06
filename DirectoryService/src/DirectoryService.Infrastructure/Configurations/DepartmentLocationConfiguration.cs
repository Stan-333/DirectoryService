using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");

        builder.HasKey(dl => new { dl.DepartmentId, dl.LocationId })
            .HasName("pk_department_location");

        builder.Property(dl => dl.DepartmentId)
            .HasConversion(dl => dl.Value, dl => new DepartmentId(dl))
            .HasColumnName("department_id");

        builder.Property(dl => dl.LocationId)
            .HasConversion(dl => dl.Value, dl => new LocationId(dl))
            .HasColumnName("location_id");

        builder.HasOne(dl => dl.Department)
            .WithMany(d => d.DepartmentLocations)
            .HasForeignKey(dl => dl.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_department_locations_department");

        builder.HasOne(dl => dl.Location)
            .WithMany()
            .HasForeignKey(dl => dl.LocationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_department_locations_location");
    }
}