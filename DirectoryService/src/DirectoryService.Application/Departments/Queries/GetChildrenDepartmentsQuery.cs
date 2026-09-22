using DirectoryService.Contracts.Departments.Requests;
using Shared.Core.Abstractions;

namespace DirectoryService.Application.Departments.Queries;

public record GetChildrenDepartmentsQuery(Guid ParentId, GetChildrenDepartmentsRequest Request) : IQuery;