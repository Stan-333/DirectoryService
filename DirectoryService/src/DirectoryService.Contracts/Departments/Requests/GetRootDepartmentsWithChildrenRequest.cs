namespace DirectoryService.Contracts.Departments.Requests;

public record GetRootDepartmentsWithChildrenRequest(
    int Page = 1,
    int PageSize = 20,
    int Prefetch = 3);