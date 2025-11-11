using Microsoft.AspNetCore.Http;

namespace Shared.EndpointResults;

public sealed class ErrorsResult : IResult {
    private readonly Errors _errors;

    public ErrorsResult(Error error) =>
        _errors = error.ToErrors();

    public ErrorsResult(Errors errors) =>
        _errors = errors;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (!_errors.Any())
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            return httpContext.Response.WriteAsJsonAsync(Envelope.Error(_errors));
        }

        var distinctErrorTypes = _errors
            .Select(e => e.Type)
            .Distinct()
            .ToList();

        int statusCode = distinctErrorTypes.Count > 1
            ? StatusCodes.Status500InternalServerError
            : GetStatusCodeFromErrorType(distinctErrorTypes.First());

        var envelope = Envelope.Error(_errors);

        httpContext.Response.StatusCode = statusCode;

        return httpContext.Response.WriteAsJsonAsync(envelope);
    }

    private static int GetStatusCodeFromErrorType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.VALIDATION => StatusCodes.Status400BadRequest,
            ErrorType.NOT_FOUND => StatusCodes.Status404NotFound,
            ErrorType.FAILURE => StatusCodes.Status500InternalServerError,
            ErrorType.CONFLICT => StatusCodes.Status409Conflict,
            ErrorType.AUTHENTICATION => StatusCodes.Status401Unauthorized,
            ErrorType.AUTHORIZATION => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };
}