using System.Data;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Seeding;

public class DirectorySeeder : ISeeder
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<DirectorySeeder> _logger;

    public DirectorySeeder(DirectoryServiceDbContext dbContext, ILogger<DirectorySeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting database seeding...");

        try
        {
            await SeedData(cancellationToken);
            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedData(CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken: cancellationToken);
        try
        {
            _logger.LogInformation("Clearing existing data...");

            // Очистка всех таблиц в правильном порядке (сначала дочерние, потом родительские)
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE department_positions CASCADE", cancellationToken);
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE department_locations CASCADE", cancellationToken);
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE positions CASCADE", cancellationToken);
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE departments CASCADE", cancellationToken);
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE locations CASCADE", cancellationToken);

            _logger.LogInformation("Generating locations...");
            var locations = new List<Location>();
            var usedLocationNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var usedLocationAddresses = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < SeedingConstants.LOCATION_COUNT; i++)
            {
                locations.Add(DataGenerator.GenerateRandomLocation(usedLocationNames, usedLocationAddresses));
            }

            _dbContext.Locations.AddRange(locations);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created {LocationsCount} locations", locations.Count);

            _logger.LogInformation("Generating departments...");
            var departments = new List<Department>();

            var usedIdentifiers = new HashSet<string>(StringComparer.Ordinal);

            // Создаем корневые департаменты
            int rootDepartmentCount = Math.Max(1, SeedingConstants.DEPARTMENT_COUNT / 3);
            for (int i = 0; i < rootDepartmentCount; i++)
            {
                departments.Add(DataGenerator.GenerateRandomDepartment(locations, usedIdentifiers));
            }

            // Создаем дочерние департаменты
            int remainingDepartments = SeedingConstants.DEPARTMENT_COUNT - rootDepartmentCount;
            for (int i = 0; i < remainingDepartments; i++)
            {
                var parent = departments[Random.Shared.Next(departments.Count)];
                departments.Add(DataGenerator.GenerateRandomDepartment(locations, usedIdentifiers, parent));
            }

            _dbContext.Departments.AddRange(departments);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created {DepartmentsCount} departments", departments.Count);

            _logger.LogInformation("Generating positions...");
            var positions = new List<Position>();
            for (int i = 0; i < SeedingConstants.POSITION_COUNT; i++)
            {
                positions.Add(DataGenerator.GenerateRandomPosition(departments, i + 1));
            }

            _dbContext.Positions.AddRange(positions);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created {PositionsCount} positions", positions.Count);

            _logger.LogInformation("Committing transaction...");
            await transaction.CommitAsync();

            _logger.LogInformation("Seeding completed: {LocationsCount} locations, {DepartmentsCount} departments, {PositionsCount} positions", locations.Count, departments.Count, positions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during seeding. Rolling back transaction.");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}