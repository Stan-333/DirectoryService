using Microsoft.AspNetCore.Builder;

namespace Stan333.Framework.Middlewares;

/// <summary>
/// Применение middleware
/// </summary>
public static class ExceptionMiddlewareExtensions
{
    // Подключать первым в конвейере, чтобы перехватывать исключения всех следующих middleware.
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionMiddleware>();
}