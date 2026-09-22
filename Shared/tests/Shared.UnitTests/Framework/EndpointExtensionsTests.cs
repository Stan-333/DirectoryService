using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shared.Framework.Endpoints;
using Shared.UnitTests.Framework.Endpoints;

namespace Shared.UnitTests.Framework;

public class EndpointExtensionsTests
{
    [Fact]
    public async Task MapEndpoints_maps_every_endpoint_found_in_assembly()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddEndpoints(typeof(PingEndpoint).Assembly);
        await using WebApplication app = builder.Build();
        app.MapEndpoints();
        await app.StartAsync();

        string response = await app.GetTestClient().GetStringAsync("/ping");

        response.Should().Be("pong");
    }

    [Fact]
    public void AddEndpoints_called_twice_does_not_duplicate_registrations()
    {
        var services = new ServiceCollection();

        services.AddEndpoints(typeof(PingEndpoint).Assembly);
        services.AddEndpoints(typeof(PingEndpoint).Assembly);

        services.Count(d => d.ServiceType == typeof(IEndpoint)).Should().Be(1);
    }
}