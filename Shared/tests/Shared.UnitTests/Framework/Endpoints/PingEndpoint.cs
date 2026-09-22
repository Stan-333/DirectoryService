using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Framework.Endpoints;

namespace Shared.UnitTests.Framework.Endpoints;

public sealed class PingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) => app.MapGet("/ping", () => Results.Text("pong"));
}