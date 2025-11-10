using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Positions;

public class Position
{
    public PositionId Id { get; set; }

    public PositionName Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // EF Core
    private Position() { }

    private Position(PositionName name, string? description, bool isActive, DateTime createdAt, DateTime? updatedAt)
    {
        Id = new PositionId(Guid.NewGuid());
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Result<Position, Error> Create(PositionName name, string? description, bool isActive, DateTime createdAt,
        DateTime? updatedAt)
    {
        if ((description?.Length ?? 0) > LengthConstants.LENGTH1000)
            return GeneralErrors.ValueIsInvalid("description");
        if (createdAt > DateTime.Now)
            return GeneralErrors.ValueIsInvalid("created_at");
        return new Position(name, description, isActive, createdAt.ToUniversalTime(),
            updatedAt?.ToUniversalTime());
    }
}