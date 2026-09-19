using DirectoryService.Contracts;

namespace DirectoryService.Contracts.Departments.Requests;

public record GetChildrenDepartmentsRequest(int Page = 1, int PageSize = 20)
{
    public const int MaxPageSize = PaginationConstraints.MaxPageSize;

    public int Page { get; init; } = PaginationConstraints.NormalizePage(Page, PageSize);

    public int PageSize { get; init; } = PaginationConstraints.NormalizePageSize(PageSize);
}