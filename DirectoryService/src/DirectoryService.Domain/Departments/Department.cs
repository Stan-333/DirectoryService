using System.Text.RegularExpressions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public class Department
{
    private readonly List<Department> _children = [];

    private readonly List<DepartmentLocation> _locations;

    private readonly List<DepartmentPosition> _positions;

    public Guid Id { get; private set; }

    public DepartmentName Name { get; private set; }

    public string Identifier { get; private set; }

    public Department? Parent { get; private set; }

    public string Path { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyList<Department> Children => _children;

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _locations;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _positions;

    private Department(DepartmentName name, string identifier, Department? parent, string path,
        short depth, bool isActive, DateTime createdAt, DateTime? updatedAt,
        IEnumerable<DepartmentLocation> locations,
        IEnumerable<DepartmentPosition> positions)
    {
        Id = Guid.NewGuid();
        Name = name;
        Identifier = identifier;
        Parent = parent;
        Path = path;
        Depth = depth;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        _locations = locations.ToList();
        _positions = positions.ToList();
    }

    public static Result<Department> Create(DepartmentName name, string identifier, Department? parent,
        bool isActive, DateTime createdAt, DateTime? updatedAt,
        IEnumerable<DepartmentLocation> locations,
        IEnumerable<DepartmentPosition> positions)
    {
        if (!Regex.IsMatch(identifier, "[a-zA-Z]{3,150}"))
            return Result<Department>.Failure("Invalid identifier");
        if (createdAt > DateTime.Now)
            return Result<Department>.Failure("Date of create must be less than current date");
        if (!locations.Any())
            return Result<Department>.Failure("Department must have at least one location");
        string path = parent is null ? identifier : $"{parent.Path}.{identifier}";
        return Result<Department>.Success(new Department(
            name, identifier, parent, path, Convert.ToInt16((parent?.Depth ?? 0) + 1),
            isActive, createdAt.ToUniversalTime(), updatedAt?.ToUniversalTime(), locations, positions));
    }
}