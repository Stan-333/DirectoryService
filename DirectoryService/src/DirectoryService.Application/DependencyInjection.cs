using DirectoryService.Application.Abstractions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Код ниже использует Scrutor (набор сервисов для расширения IServiceCollection)
        // Это описание инструкции, как найти реализации для интерфейсов
        // AsSelfWithInterfaces() - регистрирует интерфейсы и их реализации, как в комментариях ниже
        // services.AddScoped<ICommandHandler<Guid, CreateLocationCommand>, CreateLocationHandler>();
        // services.AddScoped<CreateLocationCommand>();
        var assembly = typeof(DependencyInjection).Assembly;
        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableToAny(typeof(ICommandHandler<,>), typeof(ICommandHandler<>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        return services;
    }
}