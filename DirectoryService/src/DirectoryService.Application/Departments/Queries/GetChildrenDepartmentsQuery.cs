using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Queries;

public record GetChildrenDepartmentsQuery(Guid ParentId, GetChildrenDepartmentsRequest Request) : IQuery;