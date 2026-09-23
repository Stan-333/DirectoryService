using DirectoryService.Application;
using DirectoryService.Infrastructure;
using Shared.Framework.Swagger;
using Shared.Framework.Validation;

namespace DirectoryService.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddWebDependencies()
            .AddApplication()
            .AddInfrastructure(configuration);

    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        // Ошибки привязки модели (битый JSON, неверный формат, нет обязательного поля) — тоже в формате Envelope.
        services.AddControllers().AddEnvelopeValidationResponses();

        services.AddOpenApiSpec("DirectoryService", "v1");

        return services;
    }
}