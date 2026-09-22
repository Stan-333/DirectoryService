using DirectoryService.Contracts.Departments.Requests;
using Shared.Core.Abstractions;

namespace DirectoryService.Application.Departments.UpdateDepartmentParent;

public record UpdateDepartmentParentCommand(Guid DepartmentId, UpdateDepartmentParentRequest Request) : ICommand;