using DirectoryService.Application;
using DirectoryService.Infrastructure;
using Shared.Framework.Swagger;

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
        services.AddControllers();

        services.AddOpenApiSpec("DirectoryService", "v1");

        return services;
    }
}