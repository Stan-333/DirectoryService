using Microsoft.AspNetCore.Routing;

namespace Stan333.Framework.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}