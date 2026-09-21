using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Stan333.SharedKernel;

namespace Stan333.Framework.Swagger;

public static class OpenApiExtensions
{
    /// <summary>
    /// Регистрирует генерацию OpenAPI-документа (Microsoft.AspNetCore.OpenApi) с заголовком и версией API.
    /// Остальные поля <see cref="OpenApiInfo"/> (контакт, описание) можно заполнить через <paramref name="configureInfo"/>.
    /// </summary>
    public static IServiceCollection AddOpenApiSpec(
        this IServiceCollection services,
        string title,
        string version,
        string documentName = "v1",
        Action<OpenApiInfo>? configureInfo = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentName);

        services.AddOpenApi(documentName, options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new OpenApiInfo { Title = title, Version = version };
                configureInfo?.Invoke(document.Info);
                return Task.CompletedTask;
            });

            // Errors сериализуется собственным конвертером в массив Error, поэтому генератор
            // не видит его структуру и выдаёт пустую схему. Описываем его явно.
            options.AddSchemaTransformer(async (schema, context, cancellationToken) =>
            {
                if (context.JsonTypeInfo.Type != typeof(Errors))
                {
                    return;
                }

                schema.Type = JsonSchemaType.Array;
                schema.Items = await context.GetOrCreateSchemaAsync(typeof(Error), null, cancellationToken);
            });
        });

        return services;
    }

    /// <summary>
    /// Публикует OpenAPI-документ по адресу /openapi/{documentName}.json и Swagger UI по адресу /swagger.
    /// </summary>
    public static WebApplication UseOpenApiUi(this WebApplication app, string documentName = "v1")
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentName);

        app.MapOpenApi();
        app.UseSwaggerUI(options => options.SwaggerEndpoint($"/openapi/{documentName}.json", documentName));

        return app;
    }
}