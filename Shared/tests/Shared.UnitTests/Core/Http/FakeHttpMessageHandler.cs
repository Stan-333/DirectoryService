using System.Net;
using System.Text;

namespace Shared.UnitTests.Core.Http;

/// <summary>
/// Подменяет сеть: отдаёт заданный ответ или бросает заданное исключение и запоминает последний запрос.
/// </summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<CancellationToken, Task<HttpResponseMessage>> _respond;

    private FakeHttpMessageHandler(Func<CancellationToken, Task<HttpResponseMessage>> respond) =>
        _respond = respond;

    public HttpMethod? LastMethod { get; private set; }

    public Uri? LastUri { get; private set; }

    public string? LastBody { get; private set; }

    public static FakeHttpMessageHandler Returns(HttpStatusCode statusCode, string body, string mediaType = "application/json") =>
        new(_ => Task.FromResult(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, mediaType),
        }));

    public static FakeHttpMessageHandler Throws(Exception exception) =>
        new(_ => Task.FromException<HttpResponseMessage>(exception));

    /// <summary>
    /// Отвечает, только когда отменят токен, — как сервер, который не отвечает.
    /// </summary>
    public static FakeHttpMessageHandler Hangs() =>
        new(async cancellationToken =>
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("Недостижимо");
        });

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastMethod = request.Method;
        LastUri = request.RequestUri;
        LastBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

        return await _respond(cancellationToken);
    }
}