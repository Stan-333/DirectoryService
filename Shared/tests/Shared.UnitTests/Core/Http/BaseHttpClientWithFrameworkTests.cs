using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Shared.Framework.EndpointResults;
using Shared.Framework.Middlewares;
using Shared.Kernel;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Shared.UnitTests.Core.Http;

/// <summary>
/// Клиент читает ответы в том виде, в каком их пишет Shared.Framework: сервисы и клиенты
/// собираются из одной библиотеки, и формат должен совпадать с обеих сторон.
/// </summary>
public sealed class BaseHttpClientWithFrameworkTests : IAsyncLifetime
{
    private static readonly Guid ItemId = Guid.NewGuid();

    private WebApplication _app = null!;
    private TestHttpClient _client = null!;

    public async Task InitializeAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        _app = builder.Build();
        _app.UseExceptionMiddleware();

        _app.MapGet("/items/found", () => new EndpointResult<Guid>(Result.Success<Guid, Errors>(ItemId)));
        _app.MapGet("/items/missing", () => new EndpointResult<Guid>(
            Result.Failure<Guid, Errors>(Error.NotFound("item.not.found", "Не найдено").ToErrors())));
        _app.MapGet("/items/page", () => new EndpointResult<PagedResult<string>>(
            Result.Success<PagedResult<string>, Errors>(new PagedResult<string>(["a"], 21, 2, 20))));
        _app.MapGet("/items/throws", IResult () => throw new InvalidOperationException("boom"));

        await _app.StartAsync();
        _client = new TestHttpClient(_app.GetTestClient());
    }

    public async Task DisposeAsync() => await _app.DisposeAsync();

    [Fact]
    public async Task Success_is_read_as_value()
    {
        Result<Guid, Errors> result = await _client.Get<Guid>("/items/found");

        result.Value.Should().Be(ItemId);
    }

    [Fact]
    public async Task Paged_result_is_read_as_value()
    {
        Result<PagedResult<string>, Errors> result = await _client.Get<PagedResult<string>>("/items/page");

        result.Value.Items.Should().Equal("a");
        result.Value.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task Failure_keeps_code_and_type()
    {
        Result<Guid, Errors> result = await _client.Get<Guid>("/items/missing");

        Error error = result.Error.Should().ContainSingle().Subject;
        error.Code.Should().Be("item.not.found");
        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Unhandled_exception_is_read_as_server_failure()
    {
        Result<Guid, Errors> result = await _client.Get<Guid>("/items/throws");

        result.Error.Should().ContainSingle().Which.Code.Should().Be("server.failure");
    }

    [Fact]
    public async Task Unknown_route_is_invalid_response_with_not_found_type()
    {
        Result<Guid, Errors> result = await _client.Get<Guid>("/unknown");

        Error error = result.Error.Should().ContainSingle().Subject;
        error.Code.Should().Be("http.response.invalid");
        error.Type.Should().Be(ErrorType.NotFound);
    }
}