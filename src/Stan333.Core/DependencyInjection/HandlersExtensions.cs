using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Stan333.Core.Abstractions;

namespace Stan333.Core.DependencyInjection;

public static class HandlersExtensions
{
    /// <summary>
    /// Регистрирует обработчики команд и запросов, а также валидаторы FluentValidation из указанных сборок.
    /// Обычно это единственный вызов, который нужен слою Application.
    /// </summary>
    public static IServiceCollection AddHandlersAndValidators(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddHandlers(assemblies);
        services.AddValidatorsFromAssemblies(assemblies);

        return services;
    }

    /// <summary>
    /// Регистрирует обработчики команд и запросов (scoped) по самому типу и по его интерфейсам:
    /// в пределах одного scope это один и тот же экземпляр.
    /// </summary>
    public static IServiceCollection AddHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableToAny(
                typeof(ICommandHandler<,>),
                typeof(ICommandHandler<>),
                typeof(IQueryHandler<,>),
                typeof(IQueryHandlerWithResult<,>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        return services;
    }
}