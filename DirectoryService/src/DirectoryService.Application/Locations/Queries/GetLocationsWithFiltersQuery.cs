using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.Requests;

namespace DirectoryService.Application.Locations.Queries;

public record GetLocationsWithFiltersQuery(GetLocationsWithFiltersRequest Request) : IQuery;