using DirectoryService.Contracts.Departments.Requests;
using Shared.Core.Abstractions;

namespace DirectoryService.Application.Departments.UpdateDepartmentLocations;

public record UpdateDepartmentLocationsCommand(Guid DepartmentId, UpdateDepartmentLocationsRequest Request) : ICommand;