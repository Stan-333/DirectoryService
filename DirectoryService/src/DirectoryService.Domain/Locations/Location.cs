using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using Shared;

namespace DirectoryService.Domain.Locations;

public sealed class Location
{
    private readonly List<DepartmentLocation> _departmentLocations;

    public LocationId Id { get; private set; }

    public LocationName Name { get; private set; }

    public Address Address { get; private set; }

    public TimeZone Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    // EF Core
    private Location() { }

    private Location(LocationId id, LocationName name, Address address, TimeZone timezone,
        bool isActive, DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Result<Location, Error> Create(LocationName name, Address address, TimeZone timezone,
        bool isActive, DateTime createdAt, LocationId? id = null)
    {
        if (createdAt > DateTime.Now)
            return GeneralErrors.ValueIsInvalid("location");
        return new Location(
            id ?? new LocationId(Guid.NewGuid()),
            name, address, timezone, isActive,
            createdAt.ToUniversalTime(), DateTime.UtcNow);
    }
}