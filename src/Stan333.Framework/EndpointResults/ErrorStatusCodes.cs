using Microsoft.AspNetCore.Http;
using Stan333.SharedKernel;

namespace Stan333.Framework.EndpointResults;

/// <summary>
/// Соответствие типов ошибок HTTP-статусам. Общее для <see cref="ErrorsResult"/> и ExceptionMiddleware,
/// чтобы ошибка из Result и ошибка из исключения давали один и тот же статус.
/// </summary>
public static class ErrorStatusCodes
{
    /// <summary>
    /// Если все ошибки одного типа, возвращает статус этого типа.
    /// Если типов несколько или список пуст, возвращает 500.
    /// </summary>
    public static int FromErrors(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        List<ErrorType> distinctErrorTypes = errors
            .Select(e => e.Type)
            .Distinct()
            .ToList();

        return distinctErrorTypes.Count == 1
            ? FromErrorType(distinctErrorTypes[0])
            : StatusCodes.Status500InternalServerError;
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