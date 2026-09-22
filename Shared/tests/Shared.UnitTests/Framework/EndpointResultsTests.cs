using System.Reflection;
using System.Text.Json;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Shared.Core.Http;
using Shared.Framework.EndpointResults;
using Shared.Kernel;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Shared.UnitTests.Framework;

public class EndpointResultsTests
{
    private static JsonSerializerOptions WebOptions => new(JsonSerializerDefaults.Web);

    [Fact]
    public void EndpointResult_describes_success_and_all_error_responses()
    {
        var builder = new RouteEndpointBuilder(null, RoutePatternFactory.Parse("/"), 0);
        MethodInfo method = typeof(EndpointResultsTests).GetMethod(nameof(ExecuteAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

        EndpointResult<Guid>.PopulateMetadata(method, builder);

        List<ProducesResponseTypeMetadata> produces = builder.Metadata.OfType<ProducesResponseTypeMetadata>().ToList();
        produces.Should().ContainSingle(m => m.StatusCode == 200).Which.Type.Should().Be(typeof(Envelope<Guid>));
        produces.Where(m => m.StatusCode != 200).Select(m => m.StatusCode)
            .Should().BeEquivalentTo([400, 401, 403, 404, 409, 500]);
        produces.Where(m => m.StatusCode != 200).Should().OnlyContain(m => m.Type == typeof(Envelope));
    }

    [Fact]
    public async Task ErrorsResult_writes_status_of_error_type_and_envelope()
    {
        (int status, string body) = await ExecuteAsync(new ErrorsResult(Error.Conflict("dup", "Дубликат")));

        status.Should().Be(StatusCodes.Status409Conflict);
        Envelope<Guid> envelope = JsonSerializer.Deserialize<Envelope<Guid>>(body, WebOptions)!;
        envelope.IsError.Should().BeTrue();
        envelope.ErrorList.Should().ContainSingle().Which.Code.Should().Be("dup");
    }

    [Fact]
    public async Task Error_response_has_no_result_field_and_is_readable_as_envelope_of_value_type()
    {
        (_, string body) = await ExecuteAsync(new ErrorsResult(Error.NotFound(null, "Нет")));

        using JsonDocument document = JsonDocument.Parse(body);
        document.RootElement.TryGetProperty("result", out _).Should().BeFalse();

        Envelope<Guid>? envelope = JsonSerializer.Deserialize<Envelope<Guid>>(body, WebOptions);
        envelope!.ErrorList![0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task EndpointResult_success_writes_200_and_value()
    {
        Guid id = Guid.NewGuid();
        EndpointResult<Guid> result = Result.Success<Guid, Errors>(id);

        (int status, string body) = await ExecuteAsync(result);

        status.Should().Be(StatusCodes.Status200OK);
        Envelope<Guid> envelope = JsonSerializer.Deserialize<Envelope<Guid>>(body, WebOptions)!;
        envelope.IsError.Should().BeFalse();
        envelope.Result.Should().Be(id);
    }

    [Fact]
    public async Task EndpointResult_failure_writes_status_of_errors()
    {
        EndpointResult<Guid> result = Result.Failure<Guid, Errors>(Error.Validation(null, "Плохо").ToErrors());

        (int status, _) = await ExecuteAsync(result);

        status.Should().Be(StatusCodes.Status400BadRequest);
    }

    private static async Task<(int Status, string Body)> ExecuteAsync(IResult result)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await result.ExecuteAsync(context);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return (context.Response.StatusCode, await reader.ReadToEndAsync());
    }
}