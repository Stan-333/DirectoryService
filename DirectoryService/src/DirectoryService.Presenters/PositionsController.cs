using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions.CreatePosition;
using DirectoryService.Contracts.Positions;
using Microsoft.AspNetCore.Mvc;
using Shared.Core.Abstractions;
using Shared.Framework.EndpointResults;
using Shared.Kernel;

namespace DirectoryService.Presenters;

[ApiController]
[Route("api/positions")]
public sealed class PositionsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromBody] CreatePositionRequest request,
        [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(request);

        Result<Guid, Errors> result = await handler.Handle(command, cancellationToken);

        return result;
    }
}