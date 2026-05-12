using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure;

public static class MigrationExtension
{
    public static async Task<IServiceProvider> MigrateAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();
        await dbContext.Database.MigrateAsync();
        return services;
    }
}