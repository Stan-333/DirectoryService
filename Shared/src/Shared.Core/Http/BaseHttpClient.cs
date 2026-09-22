using System.Net.Http.Json;
using System.Text.Json;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace Shared.Core.Http;

/// <summary>
/// Базовый класс типизированных HTTP-клиентов к сервисам, которые отвечают в формате <see cref="Envelope{T}"/>.
/// Ошибки сервиса возвращаются как есть, а сетевые сбои, таймауты и ответы не в формате Envelope
/// превращаются в <see cref="HttpClientErrors"/>. Исключение наружу выходит одно:
/// <see cref="OperationCanceledException"/>, когда запрос отменил сам вызывающий.
/// </summary>
/// <example>
/// <code>
/// public sealed class LocationsClient(HttpClient httpClient, ILogger&lt;LocationsClient&gt; logger)
///     : BaseHttpClient(httpClient, logger)
/// {
///     public Task&lt;Result&lt;Guid, Errors&gt;&gt; CreateAsync(CreateLocationRequest request, CancellationToken ct) =>
///         PostAsync&lt;CreateLocationRequest, Guid&gt;("api/locations", request, ct);
/// }
///
/// services.AddHttpClient&lt;LocationsClient&gt;(c => c.BaseAddress = new Uri("http://directory-service"));
/// </code>
/// </example>
public abstract class BaseHttpClient
{
    private static readonly JsonSerializerOptions DefaultJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ILogger _logger;

    protected BaseHttpClient(HttpClient httpClient, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(logger);

        HttpClient = httpClient;
        _logger = logger;
    }

    protected HttpClient HttpClient { get; }

    /// <summary>
    /// Настройки JSON для тела запроса и ответа. По умолчанию такие же, как у ASP.NET Core (camelCase).
    /// </summary>
    protected virtual JsonSerializerOptions JsonOptions => DefaultJsonOptions;

    protected async Task<Result<TResponse, Errors>> GetAsync<TResponse>(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        return await SendAsync<TResponse>(request, cancellationToken);
    }

    protected async Task<Result<TResponse, Errors>> PostAsync<TRequest, TResponse>(
        string requestUri,
        TRequest body,
        CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequestWithBody(HttpMethod.Post, requestUri, body);

        return await SendAsync<TResponse>(request, cancellationToken);
    }

    protected async Task<Result<TResponse, Errors>> PutAsync<TRequest, TResponse>(
        string requestUri,
        TRequest body,
        CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequestWithBody(HttpMethod.Put, requestUri, body);

        return await SendAsync<TResponse>(request, cancellationToken);
    }

    protected async Task<Result<TResponse, Errors>> DeleteAsync<TResponse>(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);

        return await SendAsync<TResponse>(request, cancellationToken);
    }

    /// <summary>
    /// Отправляет подготовленный запрос, например с дополнительными заголовками.
    /// Запрос не освобождается: им владеет вызывающий.
    /// </summary>
    protected async Task<Result<TResponse, Errors>> SendAsync<TResponse>(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            using HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);

            return await ReadEnvelopeAsync<TResponse>(request, response, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            // Токен вызывающего не отменён, значит сработал HttpClient.Timeout.
            _logger.LogWarning(ex, "Истекло время ожидания ответа на {Method} {Uri}", request.Method, request.RequestUri);

            return Result.Failure<TResponse, Errors>(HttpClientErrors.Timeout().ToErrors());
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Не удалось выполнить запрос {Method} {Uri}", request.Method, request.RequestUri);

            return Result.Failure<TResponse, Errors>(HttpClientErrors.RequestFailed().ToErrors());
        }
    }

    private HttpRequestMessage CreateRequestWithBody<TRequest>(HttpMethod method, string requestUri, TRequest body) =>
        new(method, requestUri) { Content = JsonContent.Create(body, options: JsonOptions) };

    private async Task<Result<TResponse, Errors>> ReadEnvelopeAsync<TResponse>(
        HttpRequestMessage request,
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        // Result читается как JsonElement, чтобы отличить отсутствующий результат от значения по умолчанию:
        // иначе чужой JSON с кодом 200 превратился бы, например, в Guid.Empty.
        Envelope<JsonElement?>? envelope;
        TResponse? value = default;
        bool hasValue = false;
        try
        {
            envelope = await response.Content.ReadFromJsonAsync<Envelope<JsonElement?>>(JsonOptions, cancellationToken);

            if (envelope?.Result is { ValueKind: not JsonValueKind.Null } result)
            {
                value = result.Deserialize<TResponse>(JsonOptions);
                hasValue = value is not null;
            }
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException or ArgumentException)
        {
            // HTML от прокси, пустое тело, чужой JSON или данные, которые не прошли проверку в конструкторе DTO.
            _logger.LogWarning(
                ex,
                "Ответ на {Method} {Uri} со статусом {StatusCode} не в формате Envelope",
                request.Method,
                request.RequestUri,
                (int)response.StatusCode);

            return Result.Failure<TResponse, Errors>(HttpClientErrors.InvalidResponse(response.StatusCode).ToErrors());
        }

        if (envelope?.ErrorList is { Count: > 0 } errors)
            return Result.Failure<TResponse, Errors>(errors);

        if (!response.IsSuccessStatusCode || !hasValue)
        {
            _logger.LogWarning(
                "Ответ на {Method} {Uri} со статусом {StatusCode} не содержит ни результата, ни ошибок",
                request.Method,
                request.RequestUri,
                (int)response.StatusCode);

            return Result.Failure<TResponse, Errors>(HttpClientErrors.InvalidResponse(response.StatusCode).ToErrors());
        }

        return Result.Success<TResponse, Errors>(value!);
    }
}