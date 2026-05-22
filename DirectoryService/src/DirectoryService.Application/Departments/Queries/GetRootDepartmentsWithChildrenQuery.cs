using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Queries;

public record GetRootDepartmentsWithChildrenQuery(GetRootDepartmentsWithChildrenRequest Request) : IQuery;