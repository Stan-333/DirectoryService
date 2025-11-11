namespace DirectoryService.web.Middlewares;

/// <summary>
/// Применение middleware
/// </summary>
public static class ExceptionMiddlewareExtensions
{
    // этот интерфейс реализует WebApplication, здесь мы его расширяем
    public static IApplicationBuilder UseExceptionMiddleware(this WebApplication app) =>
        app.UseMiddleware<ExceptionMiddleware>();
}