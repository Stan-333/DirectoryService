namespace DirectoryService.Contracts.Departments;

public record DepartmentWithPositionCountDto
{
    public Guid DepartmentId { get; init; }

    public required string DepartmentName { get; init; }

    public required string Path { get; init; }

    public DateTime CreatedAt { get; init; }

    public int PositionCount { get; init; }
}