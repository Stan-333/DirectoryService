using Microsoft.AspNetCore.Http;
using Shared.Kernel;

namespace Shared.Framework.EndpointResults;

/// <summary>
/// Соответствие типов ошибок HTTP-статусам. Общее для <see cref="ErrorsResult"/> и ExceptionMiddleware,
/// чтобы ошибка из Result и ошибка из исключения давали один и тот же статус.
/// </summary>
public static class ErrorStatusCodes
{
    /// <summary>
    /// Статус для набора ошибок. Пустой список или хотя бы одна ошибка <see cref="ErrorType.Failure"/> дают 500.
    /// Иначе берётся статус первой ошибки: смесь ошибок клиента (например, Validation и NotFound)
    /// остаётся ошибкой клиента, а не превращается в 500.
    /// </summary>
    public static int FromErrors(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        Error? first = null;
        foreach (Error error in errors)
        {
            if (error.Type == ErrorType.Failure)
            {
                return StatusCodes.Status500InternalServerError;
            }

            first ??= error;
        }

        return first is null
            ? StatusCodes.Status500InternalServerError
            : FromErrorType(first.Type);
    }

    public static int FromErrorType(ErrorType errorType) =>
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