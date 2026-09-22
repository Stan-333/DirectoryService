using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Framework.EndpointResults;
using Shared.Kernel;
using Shared.Kernel.Exceptions;

namespace Shared.Framework.Middlewares;

/// <summary>
/// Перехватывает необработанные исключения и отвечает в формате <see cref="Envelope"/>.
/// Для <see cref="AppException"/> статус берётся из типов её ошибок. Любое другое исключение
/// превращается в 500 без подробностей для клиента, а подробности уходят в лог.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Клиент разорвал соединение: отвечать некому, и это не ошибка сервера.
            _logger.LogInformation(
                "Запрос {Method} {Path} отменён клиентом",
                context.Request.Method,
                context.Request.Path);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                // Заголовки уже отправлены, заменить ответ ошибкой нельзя.
                // Пробрасываем исключение, чтобы сервер оборвал соединение и записал его в лог.
                _logger.LogWarning(
                    "Ответ на запрос {Method} {Path} уже начат, ошибку в формате Envelope записать нельзя",
                    context.Request.Method,
                    context.Request.Path);
                throw;
            }

            await WriteErrorResponseAsync(context, exception);
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        (int statusCode, Errors errors) = exception switch
        {
            AppException appException => (ErrorStatusCodes.FromErrors(appException.Errors), appException.Errors),
            _ => (StatusCodes.Status500InternalServerError, GeneralErrors.Failure().ToErrors()),
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Необработанное исключение при обработке запроса {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Запрос {Method} {Path} завершился ошибкой {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                statusCode);
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(Envelope.Error(errors));
    }
}