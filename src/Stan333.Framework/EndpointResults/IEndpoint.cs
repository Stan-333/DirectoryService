using Microsoft.AspNetCore.Routing;

namespace Stan333.Framework.EndpointResults;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}