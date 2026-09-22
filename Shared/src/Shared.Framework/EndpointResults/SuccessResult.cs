using System.Net;
using Microsoft.AspNetCore.Http;
using Shared.Core.Http;

namespace Shared.Framework.EndpointResults;

public sealed class SuccessResult<TValue> : IResult
{
    private readonly TValue _value;

    public SuccessResult(TValue value) =>
        _value = value;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var envelope = Envelope<TValue>.Ok(_value);

        httpContext.Response.StatusCode = (int)HttpStatusCode.OK;

        return httpContext.Response.WriteAsJsonAsync(envelope);
    }
}