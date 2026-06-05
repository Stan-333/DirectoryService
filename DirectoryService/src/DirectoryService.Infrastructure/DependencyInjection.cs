using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.Infrastructure.Caching;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Infrastructure.Repositories;
using DirectoryService.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure;

public static class DependencyInjection
{
    private const string DATABASE = "DirectoryServiceDb";
    private const string REDIS = "Redis";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DepartmentCleanupOptions>(configuration.GetSection(DepartmentCleanupOptions.SECTION_NAME));

        AddCache(services, configuration);

        // DbContext
        services.AddDbContext<DirectoryServiceDbContext>((sp, options) =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var environment = sp.GetRequiredService<IHostEnvironment>();

            options
                .UseLoggerFactory(loggerFactory)
                .UseNpgsql(configuration.GetConnectionString(DATABASE));

            // Логирование значений параметров раскрывает чувствительные данные —
            // включаем только в Development.
            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
            }
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

    private static void AddCache(IServiceCollection services, IConfiguration configuration)
    {
        var cacheOptions = configuration
            .GetSection(CacheOptions.SECTION_NAME)
            .Get<CacheOptions>() ?? new CacheOptions();

        services.AddSingleton(cacheOptions);

        if (!cacheOptions.Enabled)
        {
            // Кэш отключён — Redis на старте не нужен, бизнес-логика работает напрямую из БД
            services.AddSingleton<ICacheService, NullCacheService>();
            return;
        }

        string redisConnectionString = configuration.GetConnectionString(REDIS)
                                       ?? throw new InvalidOperationException(
                                           $"Connection string '{REDIS}' is not configured.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                // Сколько по умолчанию будет хранить данные локальный кэш
                LocalCacheExpiration = TimeSpan.FromMinutes(5),

                // Сколько будет хранить данные удаленный кэш
                Expiration = TimeSpan.FromMinutes(30),
            };
        });

        services.AddSingleton<ICacheService, HybridCacheService>();
    }
}