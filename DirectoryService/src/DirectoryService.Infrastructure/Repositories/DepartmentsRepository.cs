using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
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
        try
        {
            return await _dbContext.Departments
                .SingleAsync(d => d.Id == id && d.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения подразделения из базы данных");
            return GeneralErrors.Failure(ex.Message);
        }
    }

    public async Task<bool> IsActiveIdentifierExistAsync(
        Identifier identifier,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments.AnyAsync(
            d => d.Identifier == identifier && d.IsActive, cancellationToken);
    }

    public async Task<bool> IsActiveLocationExistAsync(
        LocationId locationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations.AnyAsync(
            l => l.Id == locationId && l.IsActive, cancellationToken);
    }
}