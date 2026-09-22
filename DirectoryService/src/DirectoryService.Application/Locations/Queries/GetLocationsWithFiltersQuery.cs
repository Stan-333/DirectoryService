using DirectoryService.Contracts.Locations.Requests;
using Shared.Core.Abstractions;

namespace DirectoryService.Application.Locations.Queries;

public record GetLocationsWithFiltersQuery(GetLocationsWithFiltersRequest Request) : IQuery;