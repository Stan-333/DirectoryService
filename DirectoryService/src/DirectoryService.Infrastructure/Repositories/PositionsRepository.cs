using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Infrastructure.Repositories;

public class PositionsRepository : IPositionRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<PositionsRepository> _logger;

    public PositionsRepository(
        DirectoryServiceDbContext dbContext,
        ILogger<PositionsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Guid> AddAsync(Position position, CancellationToken cancellationToken = default)
    {
        await _dbContext.Positions.AddAsync(position, cancellationToken);
        return position.Id.Value;
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

    public async Task<Result<Position, Errors>> GetByIdAsync(
        PositionId id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Positions
                .SingleAsync(p => p.Id == id && p.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения должности из базы данных");
            return GeneralErrors.Failure(ex.Message).ToErrors();
        }
    }

    public async Task<bool> IsActiveDepartmentNameExistAsync(
        PositionName name,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Positions.AnyAsync(p => p.Name == name && p.IsActive, cancellationToken);
    }

    public async Task<bool> IsActiveDepartmentExistAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments.AnyAsync(
            d => d.Id == departmentId && d.IsActive, cancellationToken);
    }
}