using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Framework.EndpointResults;
using Shared.Framework.Middlewares;
using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Shared.UnitTests.Framework;

public class ExceptionMiddlewareTests
{
    private static JsonSerializerOptions WebOptions => new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task AppException_is_answered_with_status_and_errors_of_exception()
    {
        DefaultHttpContext context = CreateContext();
        var middleware = CreateMiddleware(_ => throw new AppException(Error.NotFound("dep.not.found", "Нет")));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        Envelope<Guid> envelope = ReadEnvelope(context);
        envelope.ErrorList.Should().ContainSingle().Which.Code.Should().Be("dep.not.found");
    }

    [Fact]
    public async Task Unknown_exception_is_answered_with_500_without_details()
    {
        DefaultHttpContext context = CreateContext();
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("secret connection string"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        ReadBody(context).Should().NotContain("secret");
        ReadEnvelope(context).ErrorList.Should().ContainSingle().Which.Code.Should().Be("server.failure");
    }

    [Fact]
    public async Task Partially_written_response_is_replaced_with_error()
    {
        DefaultHttpContext context = CreateContext();
        var middleware = CreateMiddleware(async ctx =>
        {
            await ctx.Response.WriteAsync("partial");
            throw new InvalidOperationException("boom");
        });

        await middleware.InvokeAsync(context);

        ReadBody(context).Should().NotContain("partial");
        ReadEnvelope(context).IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Request_cancelled_by_client_is_not_turned_into_error_response()
    {
        DefaultHttpContext context = CreateContext();
        using var cancellation = new CancellationTokenSource();
        context.RequestAborted = cancellation.Token;
        await cancellation.CancelAsync();
        var middleware = CreateMiddleware(ctx => throw new OperationCanceledException(ctx.RequestAborted));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ReadBody(context).Should().BeEmpty();
    }

    [Fact]
    public async Task Exception_after_response_has_started_is_rethrown()
    {
        DefaultHttpContext context = CreateContext();
        context.Features.Set<IHttpResponseFeature>(new StartedResponseFeature());
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("late"));

        Func<Task> invoke = () => middleware.InvokeAsync(context);

        await invoke.Should().ThrowAsync<InvalidOperationException>().WithMessage("late");
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static ExceptionMiddleware CreateMiddleware(RequestDelegate next) =>
        new(next, NullLogger<ExceptionMiddleware>.Instance);

    private static string ReadBody(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        return reader.ReadToEnd();
    }

    private static Envelope<Guid> ReadEnvelope(HttpContext context) =>
        JsonSerializer.Deserialize<Envelope<Guid>>(ReadBody(context), WebOptions)!;

    private sealed class StartedResponseFeature : HttpResponseFeature
    {
        public override bool HasStarted => true;
    }
}