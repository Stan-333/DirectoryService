using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using Shared;

namespace DirectoryService.Domain.Departments;

public class Department
{
    private readonly List<Department> _children = [];

    private readonly List<DepartmentLocation> _locations;

    private readonly List<DepartmentPosition> _positions;

    public DepartmentId Id { get; private set; }

    public DepartmentName Name { get; private set; }

    public string Identifier { get; private set; }

    public DepartmentId? ParentId { get; private set; }

    public Department? Parent { get; private set; }

    public string Path { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyList<Department> Children => _children;

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _locations;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _positions;

    // EF Core
    private Department() { }

    private Department(DepartmentName name, string identifier, Department? parent, string path,
        short depth, bool isActive, DateTime createdAt, DateTime? updatedAt,
        IEnumerable<DepartmentLocation> locations,
        IEnumerable<DepartmentPosition> positions)
    {
        Id = new DepartmentId(Guid.NewGuid());
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

    public static Result<Department, Error> Create(DepartmentName name, string identifier, Department? parent,
        bool isActive, DateTime createdAt, DateTime? updatedAt,
        IEnumerable<DepartmentLocation> locations,
        IEnumerable<DepartmentPosition> positions)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return GeneralErrors.ValueIsRequired("identifier");
        if (!Regex.IsMatch(identifier, "[a-zA-Z]{3,150}"))
            return GeneralErrors.ValueIsInvalid("identifier");
        if (createdAt > DateTime.Now)
            return GeneralErrors.ValueIsInvalid("created at");
        if (!locations.Any())
            return GeneralErrors.ValueIsRequired("location");
        string path = parent is null ? identifier : $"{parent.Path}.{identifier}";
        return new Department(
            name, identifier, parent, path, Convert.ToInt16((parent?.Depth ?? 0) + 1),
            isActive, createdAt.ToUniversalTime(), updatedAt?.ToUniversalTime(), locations, positions);
    }
}