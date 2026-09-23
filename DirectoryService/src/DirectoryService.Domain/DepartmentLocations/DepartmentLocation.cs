using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Domain.DepartmentLocations;

public sealed class DepartmentLocation
{
    public DepartmentId DepartmentId { get; private set; }

    public LocationId LocationId { get; private set; }

    // Навигации заполняет EF Core при Include; null! — рекомендация EF Core для обязательных навигаций.
    public Department Department { get; } = null!;

    public Location Location { get; } = null!;

    public DepartmentLocation(DepartmentId departmentId, LocationId locationId)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    // EF Core: свойства заполняются при материализации из БД.
#pragma warning disable CS8618
    private DepartmentLocation() { }
#pragma warning restore CS8618
}