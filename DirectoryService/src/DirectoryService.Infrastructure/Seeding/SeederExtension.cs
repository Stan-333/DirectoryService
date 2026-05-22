using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Seeding;

public static class SeederExtension
{
    public static async Task<IServiceProvider> RunSeeding(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<ISeeder>();
        foreach (ISeeder seeder in seeders)
        {
            await seeder.SeedAsync(cancellationToken);
        }

        return services;
    }
}