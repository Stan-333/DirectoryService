namespace DirectoryService.Contracts.Locations.Requests;

public record GetLocationsWithFiltersRequest(
    string? Search,
    bool? IsActive,
    IEnumerable<Guid>? DepartmentIds,
    PaginationRequest Pagination,
    string? SortBy = "location_name",
    string? SortDirection = "asc");