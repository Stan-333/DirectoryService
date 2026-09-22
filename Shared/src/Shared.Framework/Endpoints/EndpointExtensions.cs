using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Shared.Framework.Endpoints;

public static class EndpointExtensions
{
    /// <summary>
    /// Регистрирует все неабстрактные реализации <see cref="IEndpoint"/> из указанных сборок.
    /// Повторный вызов для тех же сборок не создаёт дубликатов.
    /// </summary>
    public static IServiceCollection AddEndpoints(this IServiceCollection services, params Assembly[] assemblies)
    {
        ServiceDescriptor[] descriptors = assemblies
            .SelectMany(assembly => assembly.DefinedTypes)
            .Where(type => type is { IsAbstract: false, IsInterface: false } && type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(descriptors);

        return services;
    }

    /// <summary>
    /// Вызывает <see cref="IEndpoint.MapEndpoint"/> у всех зарегистрированных эндпоинтов.
    /// Можно вызывать и на группе маршрутов: <c>app.MapGroup("/api").MapEndpoints()</c>.
    /// </summary>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        foreach (IEndpoint endpoint in app.ServiceProvider.GetServices<IEndpoint>())
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}