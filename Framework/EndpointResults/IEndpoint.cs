using Microsoft.AspNetCore.Routing;

namespace Framework.EndpointResults;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}