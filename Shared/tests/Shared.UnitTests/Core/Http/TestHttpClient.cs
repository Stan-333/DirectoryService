using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Core.Http;
using Shared.Kernel;

namespace Shared.UnitTests.Core.Http;

/// <summary>
/// Наследник <see cref="BaseHttpClient"/>, который открывает защищённые методы для тестов.
/// </summary>
public sealed class TestHttpClient : BaseHttpClient
{
    public TestHttpClient(HttpClient httpClient)
        : base(httpClient, NullLogger.Instance)
    {
    }

    public Task<Result<TResponse, Errors>> Get<TResponse>(string requestUri, CancellationToken cancellationToken = default) =>
        GetAsync<TResponse>(requestUri, cancellationToken);

    public Task<Result<TResponse, Errors>> Post<TRequest, TResponse>(string requestUri, TRequest body) =>
        PostAsync<TRequest, TResponse>(requestUri, body);

    public Task<Result<TResponse, Errors>> Put<TRequest, TResponse>(string requestUri, TRequest body) =>
        PutAsync<TRequest, TResponse>(requestUri, body);

    public Task<Result<TResponse, Errors>> Delete<TResponse>(string requestUri) =>
        DeleteAsync<TResponse>(requestUri);

    public Task<Result<TResponse, Errors>> Send<TResponse>(HttpRequestMessage request) =>
        SendAsync<TResponse>(request);
}