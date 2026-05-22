namespace DirectoryService.Contracts.Locations.Responses;

public record GetLocationsWithFiltersResponse(
    IReadOnlyList<GetLocationDto> Locations,
    long TotalCount);