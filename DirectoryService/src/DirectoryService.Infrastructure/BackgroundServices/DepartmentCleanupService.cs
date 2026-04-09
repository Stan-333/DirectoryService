using Dapper;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices;

public sealed class DepartmentCleanupService : IDepartmentCleanupService
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<DepartmentCleanupService> _logger;
    private readonly DepartmentCleanupOptions _options;

    public DepartmentCleanupService(
        DirectoryServiceDbContext dbContext,
        IOptions<DepartmentCleanupOptions> options,
        ILogger<DepartmentCleanupService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<int> CleanupExpiredDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        var threshold = DateTime.UtcNow.AddMonths(-_options.RetentionMonths);

        var departmentIds = await _dbContext.Departments
            .AsNoTracking()
            .Where(d => !d.IsActive && d.DeletedAt.HasValue && d.DeletedAt <= threshold)
            .OrderBy(d => d.Depth)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        int deletedCount = 0;
        foreach (var departmentId in departmentIds)
        {
            try
            {
                if (await DeleteDepartmentAsync(departmentId, threshold, cancellationToken))
                {
                    deletedCount++;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Ошибка при физическом удалении подразделения {DepartmentId} во время фоновой очистки",
                    departmentId);
            }
        }

        return deletedCount;
    }

    private async Task<bool> DeleteDepartmentAsync(
        DepartmentId departmentId,
        DateTime threshold,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var department = await _dbContext.Departments
            .SingleOrDefaultAsync(d => d.Id == departmentId && !d.IsActive, cancellationToken);

        if (department?.DeletedAt is null || department.DeletedAt > threshold)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await LockDepartmentTreeAsync(department.Path, transaction, cancellationToken);

        string oldPath = department.Path;
        var newParentId = department.ParentId?.Value;
        string? newParentPath = null;

        if (department.ParentId is not null)
        {
            newParentPath = await _dbContext.Departments
                .Where(d => d.Id == department.ParentId)
                .Select(d => d.Path)
                .SingleAsync(cancellationToken);
        }

        await ReparentDirectChildrenAsync(departmentId.Value, newParentId, transaction, cancellationToken);
        await UpdateDescendantPathsAsync(oldPath, newParentPath, transaction, cancellationToken);

        _dbContext.Departments.Remove(department);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Физически удалено подразделение {DepartmentId}. Новый родитель для дочерних подразделений: {ParentId}",
            departmentId.Value,
            newParentId);

        return true;
    }

    private async Task LockDepartmentTreeAsync(
        string departmentPath,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT 1
                           FROM departments
                           WHERE path <@ @departmentPath::ltree
                           FOR UPDATE;
                           """;

        var command = new CommandDefinition(
            sql,
            new { departmentPath },
            transaction: transaction.GetDbTransaction(),
            cancellationToken: cancellationToken);

        await _dbContext.Database.GetDbConnection().ExecuteAsync(command);
    }

    private async Task ReparentDirectChildrenAsync(
        Guid departmentId,
        Guid? newParentId,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           UPDATE departments
                           SET parent_id = @newParentId,
                               updated_at = now()
                           WHERE parent_id = @departmentId;
                           """;

        var command = new CommandDefinition(
            sql,
            new { departmentId, newParentId },
            transaction: transaction.GetDbTransaction(),
            cancellationToken: cancellationToken);

        await _dbContext.Database.GetDbConnection().ExecuteAsync(command);
    }

    private async Task UpdateDescendantPathsAsync(
        string oldPath,
        string? newParentPath,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           UPDATE departments
                           SET path = CASE
                                          WHEN @newParentPath IS NULL
                                              THEN subpath(path, nlevel(@oldPath::ltree))
                                          ELSE (@newParentPath::text || '.' || subpath(path, nlevel(@oldPath::ltree))::text)::ltree
                                      END,
                               depth = CASE
                                           WHEN @newParentPath IS NULL
                                               THEN nlevel(subpath(path, nlevel(@oldPath::ltree))) - 1
                                           ELSE nlevel((@newParentPath::text || '.' || subpath(path, nlevel(@oldPath::ltree))::text)::ltree) - 1
                                       END,
                               updated_at = now()
                           WHERE path <@ @oldPath::ltree
                             AND path <> @oldPath::ltree;
                           """;

        var command = new CommandDefinition(
            sql,
            new { oldPath, newParentPath },
            transaction: transaction.GetDbTransaction(),
            cancellationToken: cancellationToken);

        await _dbContext.Database.GetDbConnection().ExecuteAsync(command);
    }
}