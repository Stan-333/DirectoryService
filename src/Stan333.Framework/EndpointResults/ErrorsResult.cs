using Microsoft.AspNetCore.Http;
using Stan333.SharedKernel;

namespace Stan333.Framework.EndpointResults;

public sealed class ErrorsResult : IResult {
    private readonly Errors _errors;

    public ErrorsResult(Error error) =>
        _errors = error.ToErrors();

    public ErrorsResult(Errors errors) =>
        _errors = errors;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var envelope = Envelope.Error(_errors);

        httpContext.Response.StatusCode = ErrorStatusCodes.FromErrors(_errors);

        return httpContext.Response.WriteAsJsonAsync(envelope);
    }
}