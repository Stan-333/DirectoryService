using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

    public async Task<Result<Department, Error>> GetByIdWithLockAsync(
        DepartmentId id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlAsync(
                $"SELECT 1 FROM departments WHERE department_id = {id.Value} AND is_active = true FOR UPDATE;",
                cancellationToken);

            var department = await _dbContext.Departments
                .SingleOrDefaultAsync(d => d.Id == id && d.IsActive, cancellationToken);

            return department == null
                ? GeneralErrors.NotFound(id.Value, nameof(Department))
                : department!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения подразделения {DepartmentId} с блокировкой из базы данных", id.Value);
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
            _logger.LogError(
                ex,
                "Ошибка удаления локаций у подразделения с id {DepartmentId}.",
                departmentId.Value);
            return GeneralErrors.Failure(ex.Message);
        }
    }

    public async Task<Result<bool, Error>> IsParent(
        string parentPath,
        DepartmentId childId,
        CancellationToken cancellationToken = default)
    {
        const string sqlCommand = """
                                  SELECT EXISTS(SELECT 1
                                  FROM departments
                                  WHERE
                                    "path" <@ @parentPath::ltree
                                    AND department_id = @departmentId
                                    AND is_active = TRUE);
                                  """;
        var command = new CommandDefinition(
            sqlCommand,
            new { parentPath, departmentId = childId.Value },
            transaction: _dbContext.Database.CurrentTransaction?.GetDbTransaction(),
            cancellationToken: cancellationToken);
        try
        {
            return await _dbContext.Database.GetDbConnection().ExecuteScalarAsync<bool>(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка проверки подразделений на подчинённость");
            return GeneralErrors.Failure(ex.Message);
        }
    }

    public async Task<UnitResult<Error>> LockDescendantsAsync(
        string parentPath,
        CancellationToken cancellationToken = default)
    {
        const string sqlCommand = """
                                  SELECT 1
                                  FROM departments
                                  WHERE
                                    "path" <@ @parentPath::ltree
                                    AND nlevel("path") > nlevel(@parentPath::ltree)
                                    AND is_active = true
                                  FOR UPDATE;
                                  """;
        var command = new CommandDefinition(
            sqlCommand,
            new { parentPath },
            transaction: _dbContext.Database.CurrentTransaction?.GetDbTransaction(),
            cancellationToken: cancellationToken);
        try
        {
            await _dbContext.Database.GetDbConnection().ExecuteAsync(command);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка блокировки подчинённых подразделений");
            return GeneralErrors.Failure(ex.Message);
        }
    }

    public async Task<UnitResult<Error>> UpdateDescendantProperties(
        string oldParentPath,
        string newParentPath,
        CancellationToken cancellationToken = default)
    {
        const string sqlCommand = """
                                  UPDATE departments
                                  SET
                                    "path" = @newParentPath::ltree || subpath("path", nlevel(@oldParentPath::ltree), nlevel("path")),
                                    "depth" = "depth" + (nlevel(@newParentPath::ltree)-nlevel(@oldParentPath::ltree)),
                                    updated_at = now()
                                  WHERE
                                    "path" <@ @oldParentPath::ltree
                                    AND nlevel("path") > nlevel(@oldParentPath::ltree)
                                    AND is_active = TRUE;
                                  """;
        var command = new CommandDefinition(
            sqlCommand,
            new { newParentPath, oldParentPath },
            transaction: _dbContext.Database.CurrentTransaction?.GetDbTransaction(),
            cancellationToken: cancellationToken);
        try
        {
            await _dbContext.Database.GetDbConnection().ExecuteAsync(command);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка проверки подразделений на подчинённость");
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