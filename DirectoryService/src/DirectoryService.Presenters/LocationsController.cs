using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Contracts.Locations;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.EndpointResults;

namespace DirectoryService.Presenters;

[ApiController]
[Route("api/location")]
public sealed class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromBody] CreateLocationRequest request,
        [FromServices] CreateLocationHandler handler,
        CancellationToken cancellationToken)
    {
        Result<Guid, Errors> result = await handler.Handle(request, cancellationToken);

        return result;
    }
}