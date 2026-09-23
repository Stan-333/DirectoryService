namespace DirectoryService.Contracts.Locations.Requests;

// Pagination необязательна: запрос без параметров пагинации возвращает первую страницу размера по умолчанию.
public record GetLocationsWithFiltersRequest(
    string? Search,
    bool? IsActive,
    IEnumerable<Guid>? DepartmentIds,
    PaginationRequest? Pagination = null,
    string? SortBy = "location_name",
    string? SortDirection = "asc");