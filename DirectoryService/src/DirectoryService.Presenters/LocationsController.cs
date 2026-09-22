using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Application.Locations.Queries;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.Requests;
using Microsoft.AspNetCore.Mvc;
using Shared.Core.Abstractions;
using Shared.Core.Http;
using Shared.Framework.EndpointResults;
using Shared.Kernel;

namespace DirectoryService.Presenters;

[ApiController]
[Route("api/locations")]
public sealed class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromBody] CreateLocationRequest request,
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(request);

        Result<Guid, Errors> result = await handler.Handle(command, cancellationToken);

        return result;
    }

    [HttpGet]
    public async Task<Envelope<PagedResult<GetLocationDto>>> GetByFilters(
        [FromQuery] GetLocationsWithFiltersRequest request,
        [FromServices] IQueryHandler<PagedResult<GetLocationDto>, GetLocationsWithFiltersQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationsWithFiltersQuery(request);

        var result = await handler.Handle(query, cancellationToken);

        return Envelope<PagedResult<GetLocationDto>>.Ok(result);
    }
}