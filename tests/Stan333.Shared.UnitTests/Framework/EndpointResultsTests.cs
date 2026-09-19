using System.Text.Json;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Stan333.Framework.EndpointResults;
using Stan333.SharedKernel;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Stan333.Shared.UnitTests.Framework;

public class EndpointResultsTests
{
    private static JsonSerializerOptions WebOptions => new(JsonSerializerDefaults.Web);

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

    [Fact]
    public void Envelope_keeps_original_time_after_deserialization()
    {
        Envelope<string> original = Envelope<string>.Ok("value");

        string json = JsonSerializer.Serialize(original, WebOptions);
        Envelope<string> restored = JsonSerializer.Deserialize<Envelope<string>>(json, WebOptions)!;

        restored.TimeGenerated.Should().Be(original.TimeGenerated);
        restored.Result.Should().Be("value");
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