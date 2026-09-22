using Microsoft.AspNetCore.Routing;

namespace Shared.Framework.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}