using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeZone = DirectoryService.Domain.Locations.TimeZone;

namespace DirectoryService.Infrastructure.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(x => x.Id).HasName("pk_location");

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new LocationId(x))
            .HasColumnName("location_id");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(LengthConstants.LENGTH120)
            .HasColumnName("location_name")
            .HasConversion(x => x.Value, x => LocationName.Create(x).Value);

        builder.ComplexProperty(x => x.Address, ab =>
        {
            ab.Property(a => a.PostalCode)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH6)
                .HasColumnName("postal_code");
            ab.Property(a => a.Region)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH100)
                .HasColumnName("region");
            ab.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH100)
                .HasColumnName("city");
            ab.Property(a => a.Street)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH100)
                .HasColumnName("street");
            ab.Property(a => a.House)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH10)
                .HasColumnName("house");
            ab.Property(a => a.Apartment)
                .HasMaxLength(LengthConstants.LENGTH10)
                .HasColumnName("apartment");
        });

        builder.Property(x => x.Timezone)
            .IsRequired()
            .HasColumnName("timezone")
            .HasConversion(x => x.Value, x => TimeZone.Create(x).Value);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.Navigation(l => l.DepartmentLocations)
            .HasField("_departmentLocations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}