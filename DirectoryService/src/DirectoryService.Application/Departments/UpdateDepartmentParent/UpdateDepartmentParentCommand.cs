using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.UpdateDepartmentParent;

public record UpdateDepartmentParentCommand(Guid DepartmentId, UpdateDepartmentParentRequest Request) : ICommand;