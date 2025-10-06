using DirectoryService.Domain.Shared;

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

    public Result<Position> Create(PositionName name, string? description, bool isActive, DateTime createdAt,
        DateTime? updatedAt)
    {
        if ((description?.Length ?? 0) > 1000)
            return Result<Position>.Failure("Description must be less than 1000 characters");
        if (createdAt > DateTime.Now)
            return Result<Position>.Failure("Date of create must be less than current date");
        return Result<Position>.Success(new Position(name, description, isActive, createdAt.ToUniversalTime(),
            updatedAt?.ToUniversalTime()));
    }
}