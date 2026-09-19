using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Stan333.Framework.Swagger;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiSpec(
        this IServiceCollection services,
        string title,
        string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        services.AddOpenApi();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = title,
                Version = version,
                Contact = new OpenApiContact
                {
                    Name = "Stan",
                    Email = "email@domain.com",
                },
            });
        });

        return services;
    }
}