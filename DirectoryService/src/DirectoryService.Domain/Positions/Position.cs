using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;
using Shared;

namespace DirectoryService.Domain.Positions;

public sealed class Position
{
    public const int DESCRIPTION_MAX_LENGTH = 1000;

    private readonly List<DepartmentPosition> _departmentPositions;

    public PositionId Id { get; set; }

    public PositionName Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    // EF Core
    private Position() { }

    private Position(
        PositionId id,
        PositionName name,
        string? description,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        IEnumerable<DepartmentPosition> departmentPositions)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        _departmentPositions = departmentPositions.ToList();
    }

    public static Result<Position, Error> Create(
        PositionName name,
        string? description,
        IEnumerable<DepartmentPosition> departmentPositions,
        PositionId? positionId = null)
    {
        if ((description?.Length ?? 0) > DESCRIPTION_MAX_LENGTH)
            return GeneralErrors.ValueIsInvalid("description");

        var departmentPositionList = departmentPositions.ToList();
        if (departmentPositionList.Count == 0)
        {
            return Error.Validation(
                "value.is.required",
                "Список DepartmentPositions должен содержать хотя бы одно подразделение");
        }

        return new Position(
            positionId ?? new PositionId(Guid.NewGuid()),
            name,
            description,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow,
            departmentPositionList);
    }
}