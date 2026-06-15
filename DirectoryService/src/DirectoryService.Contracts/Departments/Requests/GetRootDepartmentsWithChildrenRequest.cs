namespace DirectoryService.Contracts.Departments.Requests;

public record GetRootDepartmentsWithChildrenRequest(int Page = 1, int PageSize = 20, int Prefetch = 3)
{
    public const int MaxPageSize = 100;

    public int Page { get; init; } = Page < 1 ? 1 : Page;

    public int PageSize { get; init; } = Math.Clamp(PageSize, 1, MaxPageSize);

    public int Prefetch { get; init; } = Prefetch < 0 ? 0 : Prefetch;
}