using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Infrastructure.Repositories;

public class DepartmentsRepository : IDepartmentRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<DepartmentsRepository> _logger;

    public DepartmentsRepository(DirectoryServiceDbContext dbContext, ILogger<DepartmentsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Guid> AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        await _dbContext.Departments.AddAsync(department, cancellationToken);
        return department.Id.Value;
    }

    public async Task<UnitResult<Errors>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка сохранения изменений в базе данных");
            return GeneralErrors.Failure(ex.Message).ToErrors();
        }
    }

    public async Task<Result<Department, Error>> GetByIdAsync(
        DepartmentId id,
        CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments.FindAsync([id], cancellationToken);
        if (department is not null)
        {
            return department;
        }

        _logger.LogError("Подразделение с идентификатором {id} не найдено", id.Value);
        return GeneralErrors.NotFound(id.Value, nameof(Department));
    }

    public async Task<Result<Department, Error>> GetByIdWithDepartmentLocationsAsync(
        DepartmentId id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .SingleAsync(d => d.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения подразделения из базы данных");
            return GeneralErrors.Failure(ex.Message);
        }
    }

    public async Task<bool> IsActiveDepartmentExistAsync(DepartmentId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments.AnyAsync(
            d => d.Id == id && d.IsActive, cancellationToken);
    }

    public async Task<bool> IsActiveIdentifierExistAsync(
        Identifier identifier,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments.AnyAsync(
            d => d.Identifier == identifier && d.IsActive, cancellationToken);
    }

    public async Task<bool> IsActiveLocationsExistAsync(List<LocationId> locationIds, CancellationToken cancellationToken = default)
    {
        int queryResult = await _dbContext.Locations
            .Where(l => locationIds.Contains(l.Id) && l.IsActive)
            .CountAsync(cancellationToken);
        return queryResult == locationIds.Count;
    }

    public async Task<UnitResult<Error>> DeleteDepartmentLocationsByIdAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.DepartmentLocations
                .Where(dl => dl.DepartmentId == departmentId)
                .ExecuteDeleteAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Ошибка удаления локаций у подразделения с id {DepartmentId}.",
                departmentId.Value);
            return GeneralErrors.Failure(ex.Message);
        }
    }

    public async Task<UnitResult<Error>> AddDepartmentLocationsAsync(
        DepartmentId departmentId,
        List<LocationId> locationIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.DepartmentLocations
                .AddRangeAsync(
                    locationIds.Select(locId => new DepartmentLocation(departmentId, locId)), cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка добавления локаций подразделения в базу данных");
            return GeneralErrors.Failure(ex.Message);
        }
    }
}