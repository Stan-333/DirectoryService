using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Repositories;
using DirectoryService.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure;

public static class DependencyInjection
{
    private const string DATABASE = "DirectoryServiceDb";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DepartmentCleanupOptions>(configuration.GetSection(DepartmentCleanupOptions.SECTION_NAME));

        // DbContext
        services.AddDbContext<DirectoryServiceDbContext>((sp, options) =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            options
                .UseLoggerFactory(loggerFactory)
                .EnableSensitiveDataLogging()
                .UseNpgsql(configuration.GetConnectionString(DATABASE));
        });

        // Read DbContext abstraction
        services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<DirectoryServiceDbContext>());

        // Connection factory
        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        // Repositories
        services.AddScoped<ILocationsRepository, LocationsRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentsRepository>();
        services.AddScoped<IPositionRepository, PositionsRepository>();

        // Unit of Work / Transactions
        services.AddScoped<ITransactionManager, TransactionManager>();

        // Seeding
        services.AddScoped<ISeeder, DirectorySeeder>();

        // CleanupService
        services.AddScoped<IDepartmentCleanupService, DepartmentCleanupService>();
        services.AddHostedService<DepartmentCleanupBackgroundService>();

        return services;
    }
}