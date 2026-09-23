using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework.EndpointResults;
using Shared.Kernel;

namespace Shared.UnitTests.Framework.Controllers;

[ApiController]
[Route("mvc/items")]
public sealed class ItemsController : ControllerBase
{
    [HttpPost]
    public EndpointResult<Guid> Create(CreateItemRequest request) =>
        Result.Success<Guid, Errors>(Guid.NewGuid());

    [HttpGet]
    public EndpointResult<Guid> Find([FromQuery] Guid? id) =>
        Result.Success<Guid, Errors>(id ?? Guid.Empty);
}