namespace DirectoryService.Contracts.Departments.Requests;

public record UpdateDepartmentLocationsRequest(List<Guid> LocationIds);