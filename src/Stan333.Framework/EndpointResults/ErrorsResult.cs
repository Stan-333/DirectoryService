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

        var distinctErrorTypes = _errors
            .Select(e => e.Type)
            .Distinct()
            .ToList();

        int statusCode = distinctErrorTypes.Count == 1
            ? GetStatusCodeFromErrorType(distinctErrorTypes[0])
            : StatusCodes.Status500InternalServerError;

        var envelope = Envelope.Error(_errors);

        httpContext.Response.StatusCode = statusCode;

        return httpContext.Response.WriteAsJsonAsync(envelope);
    }

    private static int GetStatusCodeFromErrorType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Authentication => StatusCodes.Status401Unauthorized,
            ErrorType.Authorization => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };
}