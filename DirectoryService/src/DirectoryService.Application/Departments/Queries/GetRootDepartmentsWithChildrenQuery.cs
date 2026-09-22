using DirectoryService.Contracts.Departments.Requests;
using Shared.Core.Abstractions;

namespace DirectoryService.Application.Departments.Queries;

public record GetRootDepartmentsWithChildrenQuery(GetRootDepartmentsWithChildrenRequest Request) : IQuery;