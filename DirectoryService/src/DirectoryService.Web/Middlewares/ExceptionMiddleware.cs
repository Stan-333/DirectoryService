using System.Text.Json;
using DirectoryService.Application.Exceptions;
using Shared;
using Shared.EndpointResults;

namespace DirectoryService.web.Middlewares;

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
            // Вызов следующего middleware
            // Если в следующем коде выбросится исключение, то оно будет перехвачено catch
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, exception.Message);

        (int code, Error[]? errors) = exception switch
        {
            BadRequestException ex => (
                StatusCodes.Status500InternalServerError, JsonSerializer.Deserialize<Error[]>(exception.Message)),

            NotFoundException ex => (
                StatusCodes.Status404NotFound, JsonSerializer.Deserialize<Error[]>(exception.Message)),

            _ => (StatusCodes.Status500InternalServerError, [Error.Failure(null, "Something went wrong")]),
        };

        var envelope = Envelope.Error(new Errors(errors ?? []));

        // ответ будет в json формате
        context.Response.ContentType = "application/json";

        // код ответа
        context.Response.StatusCode = code;

        // запись ошибок в ответ
        await context.Response.WriteAsJsonAsync(envelope);
    }
}