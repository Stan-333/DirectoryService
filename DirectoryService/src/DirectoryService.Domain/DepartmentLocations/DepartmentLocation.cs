namespace DirectoryService.Domain.DepartmentLocations;

public class DepartmentLocation
{
    public Guid DepartmentId { get; }

    public Guid LocationId { get; }

    public DepartmentLocation(Guid departmentId, Guid locationId)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }
}