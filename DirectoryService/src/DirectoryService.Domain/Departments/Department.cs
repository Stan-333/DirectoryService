using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using Shared;

namespace DirectoryService.Domain.Departments;

public sealed class Department
{
    private readonly List<Department> _children = [];

    private readonly List<DepartmentLocation> _locations;

    private readonly List<DepartmentPosition> _positions;

    public DepartmentId Id { get; private set; }

    public DepartmentName Name { get; private set; }

    public Identifier Identifier { get; private set; }

    public DepartmentId? ParentId { get; private set; }

    public Department? Parent { get; private set; }

    public string Path { get; private set; }

    public short Depth { get; private set; }

    public int ChildrenCount { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<Department> Children => _children;

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _locations;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _positions;

    // EF Core
    private Department() { }

    private Department(
        DepartmentId id,
        DepartmentName name,
        Identifier identifier,
        Department? parent,
        string path,
        short depth,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        IEnumerable<DepartmentLocation> locations)
    {
        Id = id;
        Name = name;
        Identifier = identifier;
        Parent = parent;
        Path = path;
        Depth = depth;
        ChildrenCount = Children.Count;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        _locations = locations.ToList();
    }

    public static Result<Department, Error> CreateParent(
        DepartmentName name,
        Identifier identifier,
        IEnumerable<DepartmentLocation> locations,
        DepartmentId? departmentId = null)
    {
        var departmentLocationList = locations.ToList();
        if (departmentLocationList.Count == 0)
        {
            return Error.Validation(
                "value.is.required",
                "Список DepartmentLocations должен содержать хотя бы одну локацию");
        }

        return new Department(
            departmentId ?? new DepartmentId(Guid.NewGuid()),
            name,
            identifier,
            null,
            identifier.Value,
            0,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow,
            departmentLocationList);
    }

    public static Result<Department, Error> CreateChild(
        DepartmentName name,
        Identifier identifier,
        Department parent,
        IEnumerable<DepartmentLocation> locations,
        DepartmentId? departmentId = null)
    {
        var departmentLocationList = locations.ToList();
        if (departmentLocationList.Count == 0)
        {
            return Error.Validation(
                "value.is.required",
                "Список DepartmentLocations должен содержать хотя бы одну локацию");
        }

        string path = $"{parent.Path}.{identifier.Value}";
        return new Department(
            departmentId ?? new DepartmentId(Guid.NewGuid()),
            name,
            identifier,
            parent,
            path,
            Convert.ToInt16((parent?.Depth ?? 0) + 1),
            true,
            DateTime.UtcNow,
            DateTime.UtcNow,
            departmentLocationList);
    }
}