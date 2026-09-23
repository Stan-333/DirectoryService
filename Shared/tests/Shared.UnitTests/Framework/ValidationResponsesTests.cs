using System.Net;
using System.Text;
using System.Text.Json;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Core.Http;
using Shared.Framework.Endpoints;
using Shared.Framework.Middlewares;
using Shared.Framework.Validation;
using Shared.Kernel;
using Shared.UnitTests.Core.Http;
using Shared.UnitTests.Framework.Controllers;

namespace Shared.UnitTests.Framework;

/// <summary>
/// Ошибки привязки модели (до хендлера) отвечают в формате Envelope — и в MVC, и в minimal API.
/// Окружение Production: там minimal API по умолчанию отвечает на ошибку привязки пустым 400.
/// </summary>
public sealed class ValidationResponsesTests : IAsyncLifetime
{
    private static JsonSerializerOptions WebOptions => new(JsonSerializerDefaults.Web);

    private WebApplication _app = null!;
    private HttpClient _http = null!;

    public async Task InitializeAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = Environments.Production });
        builder.WebHost.UseTestServer();
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(ItemsController).Assembly)
            .AddEnvelopeValidationResponses();
        builder.Services.AddEndpoints();

        _app = builder.Build();
        _app.UseExceptionMiddleware();
        _app.MapControllers();
        _app.MapPost("/minimal/items", (CreateItemRequest request) => Results.Ok(request.Name));

        await _app.StartAsync();
        _http = _app.GetTestClient();
    }

    public async Task DisposeAsync() => await _app.DisposeAsync();

    [Fact]
    public async Task Mvc_missing_required_field_is_validation_error_with_field_name()
    {
        HttpResponseMessage response = await PostJsonAsync("/mvc/items", """{"quantity":1}""");

        Envelope envelope = await ReadEnvelopeAsync(response, HttpStatusCode.BadRequest);
        Error error = envelope.ErrorList.Should().ContainSingle().Subject;
        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be("value.is.invalid");
        error.InvalidField.Should().Be("Name");
    }

    [Fact]
    public async Task Mvc_broken_json_is_validation_error_in_envelope()
    {
        HttpResponseMessage response = await PostJsonAsync("/mvc/items", """{"name":""");

        Envelope envelope = await ReadEnvelopeAsync(response, HttpStatusCode.BadRequest);
        envelope.ErrorList.Should().NotBeEmpty()
            .And.OnlyContain(e => e.Type == ErrorType.Validation && e.Code == "value.is.invalid");
    }

    [Fact]
    public async Task Mvc_query_value_in_wrong_format_is_validation_error_with_field_name()
    {
        HttpResponseMessage response = await _http.GetAsync("/mvc/items?id=not-a-guid");

        Envelope envelope = await ReadEnvelopeAsync(response, HttpStatusCode.BadRequest);
        envelope.ErrorList.Should().ContainSingle().Which.InvalidField.Should().Be("id");
    }

    [Fact]
    public async Task BaseHttpClient_reads_model_validation_errors_as_errors_of_service()
    {
        var client = new TestHttpClient(_http);

        Result<Guid, Errors> result = await client.Post<CreateItemRequest, Guid>("/mvc/items", new CreateItemRequest(null!, 1));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().ContainSingle().Which.InvalidField.Should().Be("Name");
    }

    [Fact]
    public async Task Minimal_api_broken_json_is_answered_with_envelope_in_production()
    {
        HttpResponseMessage response = await PostJsonAsync("/minimal/items", """{"name":""");

        Envelope envelope = await ReadEnvelopeAsync(response, HttpStatusCode.BadRequest);
        envelope.ErrorList.Should().ContainSingle().Which.Code.Should().Be("request.is.invalid");
    }

    private static async Task<Envelope> ReadEnvelopeAsync(HttpResponseMessage response, HttpStatusCode expectedStatus)
    {
        response.StatusCode.Should().Be(expectedStatus);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        string body = await response.Content.ReadAsStringAsync();
        Envelope envelope = JsonSerializer.Deserialize<Envelope>(body, WebOptions)!;
        envelope.IsError.Should().BeTrue();
        return envelope;
    }

    private async Task<HttpResponseMessage> PostJsonAsync(string uri, string json) =>
        await _http.PostAsync(uri, new StringContent(json, Encoding.UTF8, "application/json"));
}