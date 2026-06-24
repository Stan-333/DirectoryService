namespace DirectoryService.Contracts.Departments.Requests;

public record GetChildrenDepartmentsRequest(int Page = 1, int PageSize = 20)
{
    public const int MaxPageSize = 100;

    public int Page { get; init; } = Page < 1 ? 1 : Page;

    public int PageSize { get; init; } = Math.Clamp(PageSize, 1, MaxPageSize);
}