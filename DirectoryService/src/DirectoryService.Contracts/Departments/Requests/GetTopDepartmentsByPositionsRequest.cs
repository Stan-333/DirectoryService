namespace DirectoryService.Contracts.Departments.Requests;

public record GetTopDepartmentsByPositionsRequest(int RowsCount = 5);