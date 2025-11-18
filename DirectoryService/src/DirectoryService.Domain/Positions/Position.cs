using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Positions;

public sealed class Position
{
    public PositionId Id { get; set; }

    public PositionName Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // EF Core
    private Position() { }

    private Position(PositionId id, PositionName name, string? description, bool isActive, DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Result<Position, Error> Create(PositionName name, string? description, bool isActive, DateTime createdAt,
        PositionId? positionId = null)
    {
        if ((description?.Length ?? 0) > LengthConstants.LENGTH1000)
            return GeneralErrors.ValueIsInvalid("description");
        if (createdAt > DateTime.Now)
            return GeneralErrors.ValueIsInvalid("created_at");
        return new Position(
            positionId ?? new PositionId(Guid.NewGuid()),
            name, description, isActive, createdAt.ToUniversalTime(),
            DateTime.UtcNow);
    }
}