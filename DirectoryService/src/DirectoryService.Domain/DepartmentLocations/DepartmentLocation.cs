using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Domain.DepartmentLocations;

public class DepartmentLocation
{
    public DepartmentId DepartmentId { get; private set; }

    public LocationId LocationId { get; private set; }

    public Department Department { get; }

    public Location Location { get; }

    // EF Core
    private DepartmentLocation() { }

    public DepartmentLocation(DepartmentId departmentId, LocationId locationId)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }
}