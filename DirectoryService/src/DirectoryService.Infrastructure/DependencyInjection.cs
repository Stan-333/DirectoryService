using System.Runtime.CompilerServices;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Repositories;
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
        // DbContext
        services.AddDbContext<DirectoryServiceDbContext>((sp, options) =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            options.UseNpgsql(
                configuration.GetConnectionString(DATABASE));

            options.UseLoggerFactory(loggerFactory);
        });

        // Repositories
        services.AddScoped<ILocationsRepository, LocationsRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentsRepository>();
        services.AddScoped<IPositionRepository, PositionsRepository>();

        // Unit of Work / Transactions
        services.AddScoped<ITransactionManager, TransactionManager>();
        return services;
    }
}