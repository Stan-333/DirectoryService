using System.Net;
using System.Text.Json;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Shared.Core.Http;
using Shared.Kernel;

namespace Shared.UnitTests.Core.Http;

public class BaseHttpClientTests
{
    private static JsonSerializerOptions WebOptions => new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Get_returns_result_from_envelope()
    {
        Guid id = Guid.NewGuid();
        FakeHttpMessageHandler handler = Ok(id);

        Result<Guid, Errors> result = await CreateClient(handler).Get<Guid>("api/items/1");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(id);
        handler.LastMethod.Should().Be(HttpMethod.Get);
        handler.LastUri.Should().Be(new Uri("http://service/api/items/1"));
    }

    [Fact]
    public async Task Post_and_put_send_body_as_camel_case_json()
    {
        FakeHttpMessageHandler handler = Ok("done");
        TestHttpClient client = CreateClient(handler);

        Result<string, Errors> posted = await client.Post<CreateItem, string>("api/items", new CreateItem("Store"));

        posted.Value.Should().Be("done");
        handler.LastMethod.Should().Be(HttpMethod.Post);
        handler.LastBody.Should().Be("""{"itemName":"Store"}""");

        await client.Put<CreateItem, string>("api/items/1", new CreateItem("Shop"));

        handler.LastMethod.Should().Be(HttpMethod.Put);
        handler.LastBody.Should().Be("""{"itemName":"Shop"}""");
    }

    [Fact]
    public async Task Delete_sends_delete_without_body()
    {
        FakeHttpMessageHandler handler = Ok(Guid.NewGuid());

        Result<Guid, Errors> result = await CreateClient(handler).Delete<Guid>("api/items/1");

        result.IsSuccess.Should().BeTrue();
        handler.LastMethod.Should().Be(HttpMethod.Delete);
        handler.LastBody.Should().BeNull();
    }

    [Fact]
    public async Task Send_uses_prepared_request_and_leaves_it_to_caller()
    {
        FakeHttpMessageHandler handler = Ok(1);
        using var request = new HttpRequestMessage(HttpMethod.Patch, "api/items/1") { Content = new StringContent("{}") };

        Result<int, Errors> result = await CreateClient(handler).Send<int>(request);

        result.Value.Should().Be(1);
        handler.LastMethod.Should().Be(HttpMethod.Patch);
        (await request.Content.ReadAsStringAsync()).Should().Be("{}", "запросом владеет вызывающий, клиент его не освобождает");
    }

    [Fact]
    public async Task Paged_result_is_read_from_envelope()
    {
        var page = new PagedResult<string>(["a", "b"], 45, 2, 20);
        FakeHttpMessageHandler handler = Ok(page);

        Result<PagedResult<string>, Errors> result = await CreateClient(handler).Get<PagedResult<string>>("api/items");

        result.Value.Should().BeEquivalentTo(page);
    }

    [Fact]
    public async Task Service_errors_are_returned_as_is()
    {
        Errors errors = new[]
        {
            Error.Validation("name.invalid", "Неверное имя", "name"),
            Error.Validation("city.invalid", "Неверный город", "city"),
        };
        FakeHttpMessageHandler handler = Json(HttpStatusCode.BadRequest, Envelope.Error(errors));

        Result<Guid, Errors> result = await CreateClient(handler).Get<Guid>("api/items");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(errors);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadGateway, "<html>Bad Gateway</html>", "text/html", ErrorType.Failure)]
    [InlineData(HttpStatusCode.NotFound, "", "text/plain", ErrorType.NotFound)]
    [InlineData(HttpStatusCode.Unauthorized, "", "text/plain", ErrorType.Authentication)]
    [InlineData(HttpStatusCode.Conflict, "{}", "application/json", ErrorType.Conflict)]
    [InlineData(HttpStatusCode.OK, "null", "application/json", ErrorType.Failure)]
    [InlineData(HttpStatusCode.OK, """{"items":[1,2]}""", "application/json", ErrorType.Failure)]
    [InlineData(HttpStatusCode.OK, """{"result":"not-a-guid"}""", "application/json", ErrorType.Failure)]
    public async Task Response_not_in_envelope_format_becomes_invalid_response(
        HttpStatusCode statusCode,
        string body,
        string mediaType,
        ErrorType expectedType)
    {
        FakeHttpMessageHandler handler = FakeHttpMessageHandler.Returns(statusCode, body, mediaType);

        Result<Guid, Errors> result = await CreateClient(handler).Get<Guid>("api/items");

        result.IsFailure.Should().BeTrue();
        Error error = result.Error.Should().ContainSingle().Subject;
        error.Code.Should().Be("http.response.invalid");
        error.Type.Should().Be(expectedType);
    }

    [Fact]
    public async Task Success_envelope_without_result_is_invalid_response()
    {
        FakeHttpMessageHandler handler = Ok<string?>(null);

        Result<string, Errors> result = await CreateClient(handler).Get<string>("api/items");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().ContainSingle().Which.Code.Should().Be("http.response.invalid");
    }

    [Fact]
    public async Task Invalid_dto_in_result_is_invalid_response()
    {
        const string body = """{"result":{"items":[],"totalCount":0,"page":0,"pageSize":20}}""";
        FakeHttpMessageHandler handler = FakeHttpMessageHandler.Returns(HttpStatusCode.OK, body);

        Result<PagedResult<string>, Errors> result = await CreateClient(handler).Get<PagedResult<string>>("api/items");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().ContainSingle().Which.Code.Should().Be("http.response.invalid");
    }

    [Fact]
    public async Task Network_failure_becomes_request_failed()
    {
        FakeHttpMessageHandler handler = FakeHttpMessageHandler.Throws(new HttpRequestException("Connection refused"));

        Result<Guid, Errors> result = await CreateClient(handler).Get<Guid>("api/items");

        result.IsFailure.Should().BeTrue();
        Error error = result.Error.Should().ContainSingle().Subject;
        error.Code.Should().Be("http.request.failed");
        error.Type.Should().Be(ErrorType.Failure);
        error.Message.Should().NotContain("Connection refused", "подробности исключения уходят в лог, а не клиенту");
    }

    [Fact]
    public async Task Http_client_timeout_becomes_request_timeout()
    {
        var httpClient = new HttpClient(FakeHttpMessageHandler.Hangs())
        {
            BaseAddress = new Uri("http://service/"),
            Timeout = TimeSpan.FromMilliseconds(50),
        };

        Result<Guid, Errors> result = await new TestHttpClient(httpClient).Get<Guid>("api/items");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().ContainSingle().Which.Code.Should().Be("http.request.timeout");
    }

    [Fact]
    public async Task Cancellation_by_caller_is_rethrown()
    {
        using var cancellation = new CancellationTokenSource();
        TestHttpClient client = CreateClient(FakeHttpMessageHandler.Hangs());

        Task<Result<Guid, Errors>> call = client.Get<Guid>("api/items", cancellation.Token);
        await cancellation.CancelAsync();

        await FluentActions.Awaiting(() => call).Should().ThrowAsync<OperationCanceledException>();
    }

    private static TestHttpClient CreateClient(FakeHttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("http://service/") });

    private static FakeHttpMessageHandler Ok<T>(T value) =>
        Json(HttpStatusCode.OK, Envelope<T>.Ok(value));

    private static FakeHttpMessageHandler Json<T>(HttpStatusCode statusCode, T envelope) =>
        FakeHttpMessageHandler.Returns(statusCode, JsonSerializer.Serialize(envelope, WebOptions));

    private sealed record CreateItem(string ItemName);
}