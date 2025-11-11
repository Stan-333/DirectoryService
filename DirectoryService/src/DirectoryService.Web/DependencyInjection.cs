using DirectoryService.Application;
using DirectoryService.Infrastructure;

namespace DirectoryService.web;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(this IServiceCollection services) =>
        services
            .AddWebDependencies()
            .AddApplication()
            .AddInfrastructure();

    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();

        return services;
    }
}