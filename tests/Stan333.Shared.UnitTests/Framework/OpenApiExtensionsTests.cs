using System.Net;
using System.Text.Json;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Stan333.Framework.EndpointResults;
using Stan333.Framework.Swagger;
using Stan333.SharedKernel;

namespace Stan333.Shared.UnitTests.Framework;

public class OpenApiExtensionsTests
{
    [Fact]
    public async Task Document_has_title_and_version()
    {
        await using WebApplication app = await StartAppAsync();

        using JsonDocument document = JsonDocument.Parse(await app.GetTestClient().GetStringAsync("/openapi/v1.json"));

        JsonElement info = document.RootElement.GetProperty("info");
        info.GetProperty("title").GetString().Should().Be("Test API");
        info.GetProperty("version").GetString().Should().Be("1.0");
    }

    [Fact]
    public async Task Error_list_is_described_as_array_of_errors()
    {
        await using WebApplication app = await StartAppAsync();

        using JsonDocument document = JsonDocument.Parse(await app.GetTestClient().GetStringAsync("/openapi/v1.json"));

        JsonElement schemas = document.RootElement.GetProperty("components").GetProperty("schemas");
        JsonElement errorList = Resolve(schemas, schemas.GetProperty("Envelope").GetProperty("properties").GetProperty("errorList"));
        errorList.GetProperty("type").ToString().Should().Contain("array");
        Resolve(schemas, errorList.GetProperty("items")).GetProperty("properties").TryGetProperty("code", out _)
            .Should().BeTrue();
    }

    [Fact]
    public async Task Swagger_ui_is_served()
    {
        await using WebApplication app = await StartAppAsync();

        HttpResponseMessage response = await app.GetTestClient().GetAsync("/swagger/index.html");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<WebApplication> StartAppAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddOpenApiSpec("Test API", "1.0");
        WebApplication app = builder.Build();
        app.UseOpenApiUi();
        app.MapGet("/items/{id:guid}", (Guid id) => new EndpointResult<Guid>(Result.Success<Guid, Errors>(id)));
        await app.StartAsync();
        return app;
    }

    private static JsonElement Resolve(JsonElement schemas, JsonElement schema)
    {
        if (!schema.TryGetProperty("$ref", out JsonElement reference))
        {
            return schema;
        }

        string name = reference.GetString()!.Split('/')[^1];
        return schemas.GetProperty(name);
    }
}