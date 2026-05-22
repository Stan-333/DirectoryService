namespace DirectoryService.Contracts.Departments.Requests;

public record GetChildrenDepartmentsRequest(
    int Page = 1,
    int PageSize = 20);