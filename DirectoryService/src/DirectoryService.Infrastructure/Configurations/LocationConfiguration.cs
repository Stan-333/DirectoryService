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

        builder.HasKey(l => l.Id).HasName("pk_location");

        builder.Property(l => l.Id)
            .HasConversion(locId => locId.Value, id => new LocationId(id))
            .HasColumnName("location_id");

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(LocationName.NAME_MAX_LENGTH)
            .HasColumnName("location_name")
            .HasConversion(locName => locName.Value, x => LocationName.Create(x).Value);

        builder.HasIndex(l => l.Name)
            .HasDatabaseName("idx_location_name")
            .HasFilter("is_active = true")
            .IsUnique();

        builder.ComplexProperty(l => l.Address, ab =>
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

        // Нерабочий вариант индекса (задача на будущее сделать рабочий)
        // builder.HasIndex(l => new
        // {
        //     l.Address.PostalCode,
        //     l.Address.Region,
        //     l.Address.City,
        //     l.Address.Street,
        //     l.Address.House,
        //     l.Address.Apartment,
        // }).HasDatabaseName("idx_location_address").HasFilter("is_active = true").IsUnique();

        builder.Property(l => l.Timezone)
            .IsRequired()
            .HasColumnName("timezone")
            .HasConversion(tz => tz.Value, x => TimeZone.Create(x).Value);

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(l => l.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(l => l.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.Navigation(l => l.DepartmentLocations)
            .HasField("_departmentLocations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}