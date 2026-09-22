using DirectoryService.Contracts.Departments.Requests;
using Shared.Core.Abstractions;

namespace DirectoryService.Application.Departments.Queries;

public record GetTopDepartmentsByPositionQuery(GetTopDepartmentsByPositionsRequest Request) : IQuery;