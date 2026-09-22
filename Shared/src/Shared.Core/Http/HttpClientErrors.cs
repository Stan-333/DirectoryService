using System.Net;
using Shared.Kernel;

namespace Shared.Core.Http;

/// <summary>
/// Ошибки, которые <see cref="BaseHttpClient"/> возвращает, когда сервис не прислал свои ошибки в <see cref="Envelope"/>.
/// В сообщениях нет адресов и деталей исключений: ошибка может уйти дальше, к клиенту вызывающего сервиса.
/// Подробности пишутся в лог.
/// </summary>
public static class HttpClientErrors
{
    public static Error RequestFailed() =>
        Error.Failure("http.request.failed", "Не удалось выполнить запрос к сервису");

    public static Error Timeout() =>
        Error.Failure("http.request.timeout", "Сервис не ответил вовремя");

    /// <summary>
    /// Ответ не в формате <see cref="Envelope"/>: HTML от прокси, пустое тело, чужой JSON,
    /// ответ с ошибкой без списка ошибок или успешный ответ без результата.
    /// Тип ошибки берётся из статуса ответа, чтобы, например, 404 оставался NotFound.
    /// </summary>
    public static Error InvalidResponse(HttpStatusCode statusCode)
    {
        const string code = "http.response.invalid";
        string message = $"Сервис вернул ответ в неожиданном формате (статус {(int)statusCode})";

        return statusCode switch
        {
            HttpStatusCode.BadRequest => Error.Validation(code, message),
            HttpStatusCode.Unauthorized => Error.Authentication(code, message),
            HttpStatusCode.Forbidden => Error.Authorization(code, message),
            HttpStatusCode.NotFound => Error.NotFound(code, message),
            HttpStatusCode.Conflict => Error.Conflict(code, message),
            _ => Error.Failure(code, message),
        };
    }
}