namespace DirectoryService.Contracts.Departments;

public record DepartmentWithChildrenInfoDto
{
    public Guid DepartmentId { get; init; }

    public Guid? ParentId { get; init; }

    public required string DepartmentName { get; init; }

    public required string Identifier { get; init; }

    public required string Path { get; init; }

    public required int Depth { get; init; }

    public required bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public bool HasMoreChildren { get; init; }
}