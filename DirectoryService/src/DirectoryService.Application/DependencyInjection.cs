using Microsoft.Extensions.DependencyInjection;
using Shared.Core.DependencyInjection;

namespace DirectoryService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) =>
        services.AddHandlersAndValidators(typeof(DependencyInjection).Assembly);
}