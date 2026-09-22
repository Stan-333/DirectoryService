using DirectoryService.Contracts.Departments.Requests;
using Shared.Core.Abstractions;

namespace DirectoryService.Application.Departments.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;