using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.Queries;

public record GetTopDepartmentsByPositionQuery(GetTopDepartmentsByPositionsRequest Request) : IQuery;