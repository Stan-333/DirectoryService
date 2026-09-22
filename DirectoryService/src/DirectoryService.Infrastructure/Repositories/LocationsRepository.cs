using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<LocationsRepository> _logger;

    public LocationsRepository(
        DirectoryServiceDbContext dbContext,
        ILogger<LocationsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Guid> AddAsync(Location location, CancellationToken cancellationToken = default)
    {
        await _dbContext.Locations.AddAsync(location, cancellationToken);
        return location.Id.Value;
    }

    public async Task<bool> IsActiveLocationNameExistAsync(
        LocationName locationName,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations.AnyAsync(
            l => l.Name.Value == locationName.Value && l.IsActive, cancellationToken);
    }

    public async Task<bool> IsActiveAddressExistAsync(Address address, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations.AnyAsync(
            l => l.Address == address && l.IsActive, cancellationToken);
    }

    public async Task<UnitResult<Errors>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Errors>();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            Error? error = PostgresErrorMapper.MapUniqueViolation(ex, out string? constraintName);
            if (error is not null)
            {
                _logger.LogWarning(
                    ex,
                    "Конфликт уникальности при сохранении локации. Ограничение: {ConstraintName}",
                    constraintName);
                return error.ToErrors();
            }

            _logger.LogError(ex, "Ошибка сохранения изменений в базе данных");
            return GeneralErrors.Failure().ToErrors();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка сохранения изменений в базе данных");
            return GeneralErrors.Failure().ToErrors();
        }
    }
}