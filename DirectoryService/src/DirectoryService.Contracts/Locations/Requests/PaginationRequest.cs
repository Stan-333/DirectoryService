namespace DirectoryService.Contracts.Locations.Requests;

public record PaginationRequest(int Page = 1, int PageSize = 20);