using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Locations;

public class Location
{
    public LocationId Id { get; private set; }

    public LocationName Name { get; private set; }

    public Address Address { get; private set; }

    public TimeZone Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    // EF Core
    private Location() { }

    private Location(LocationName name, Address address, TimeZone timezone,
        bool isActive, DateTime createdAt, DateTime? updatedAt)
    {
        Id = new LocationId(Guid.NewGuid());
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Result<Location, Error> Create(LocationName name, Address address, TimeZone timezone,
        bool isActive, DateTime createdAt, DateTime? updatedAt)
    {
        if (createdAt > DateTime.Now)
            return GeneralErrors.ValueIsInvalid("location");
        return new Location(name, address, timezone, isActive,
            createdAt.ToUniversalTime(), updatedAt?.ToUniversalTime());
    }
}