using DirectoryService.Application;
using DirectoryService.Infrastructure;

namespace DirectoryService.web;

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

        /*// На случай ошибки необходимо добавить код, позволяющий обрабатывать кастомный класс Error
        services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer((schema, context, _) =>
            {
                if (context.JsonTypeInfo.Type != typeof(Envelope<Errors>))
                {
                    return Task.CompletedTask;
                }

                if (schema.Properties.TryGetValue("errors", out var errorsProp))
                {
                    errorsProp.Items.Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "Error" };
                }

                return Task.CompletedTask;
            });
        });*/

        services.AddOpenApi();

        return services;
    }
}