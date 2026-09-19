namespace DirectoryService.Contracts.Departments.Requests;

public record GetTopDepartmentsByPositionsRequest(int RowsCount = 5)
{
    public const int MaxRowsCount = 100;

    public int RowsCount { get; init; } = Math.Clamp(RowsCount, 1, MaxRowsCount);
}