using DirectoryService.Contracts;

namespace DirectoryService.Contracts.Departments.Requests;

public record GetRootDepartmentsWithChildrenRequest(int Page = 1, int PageSize = 20, int Prefetch = 3)
{
    public const int MaxPageSize = PaginationConstraints.MaxPageSize;
    public const int MaxPrefetch = 100;

    public int Page { get; init; } = PaginationConstraints.NormalizePage(Page, PageSize);

    public int PageSize { get; init; } = PaginationConstraints.NormalizePageSize(PageSize);

    public int Prefetch { get; init; } = Math.Clamp(Prefetch, 0, MaxPrefetch);
}