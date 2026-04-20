using Dapper;
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
        DateTime threshold = DateTime.UtcNow.AddMonths(-_options.RetentionMonths);

        await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        const string sql = """
                           WITH 
                           -- находим подразделения-кандидаты на физическое удаление
                           candidates AS (
                               SELECT
                                   d.department_id,
                                   d.path
                               FROM departments AS d
                               WHERE d.is_active = FALSE
                                 AND d.deleted_at <= @threshold
                           ),
                           -- находим живые подразделения, которые находятся внутри удаляемых веток
                           affected_departments AS (
                               SELECT
                                   d.department_id,
                                   d.path
                               FROM departments AS d
                               LEFT JOIN candidates AS candidate
                                   ON candidate.department_id = d.department_id
                               WHERE candidate.department_id IS NULL
                                 AND EXISTS (
                                     SELECT 1
                                     FROM candidates AS candidate_ancestor
                                     WHERE candidate_ancestor.path @> d.path
                                 )
                           ),
                           -- для каждого живого подразделения заново вычисляем новые parent_id, path и depth
                           recalculated_departments AS (
                               SELECT
                                   affected.department_id,
                                   path_info.new_parent_id,
                                   path_info.new_path,
                                   path_info.new_depth
                               FROM affected_departments AS affected
                               CROSS JOIN LATERAL (
                                   SELECT
                                       string_agg(
                                           ancestor.identifier,
                                           '.'
                                           ORDER BY nlevel(ancestor.path), ancestor.path) AS new_path,
                                       (COUNT(*) - 1)::smallint AS new_depth,
                                       (array_agg(
                                           ancestor.department_id
                                           ORDER BY nlevel(ancestor.path), ancestor.path))[COUNT(*) - 1] AS new_parent_id
                                   FROM departments AS ancestor
                                   WHERE ancestor.path @> affected.path
                                     AND NOT EXISTS (
                                         SELECT 1
                                         FROM candidates AS deleted_ancestor
                                         WHERE deleted_ancestor.department_id = ancestor.department_id
                                     )
                               ) AS path_info
                           ),
                           updated_departments AS (
                               UPDATE departments AS d
                               SET parent_id = recalculated.new_parent_id,
                                   path = recalculated.new_path::ltree,
                                   depth = recalculated.new_depth,
                                   updated_at = now()
                               FROM recalculated_departments AS recalculated
                               WHERE d.department_id = recalculated.department_id
                               RETURNING d.department_id
                           ),
                           deleted_departments AS (
                               DELETE FROM departments AS d
                               USING candidates AS candidate
                               WHERE d.department_id = candidate.department_id
                               RETURNING d.department_id
                           )
                           SELECT
                               (SELECT COUNT(*) FROM updated_departments) AS updated_count,
                               (SELECT COUNT(*) FROM deleted_departments) AS deleted_count;
                           """;

        try
        {
            CleanupResult result = await _dbContext.Database.GetDbConnection().QuerySingleAsync<CleanupResult>(
                new CommandDefinition(
                    sql,
                    new { threshold },
                    transaction: transaction.GetDbTransaction(),
                    cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Фоновая очистка подразделений завершена. Обновлено записей: {UpdatedCount}. Удалено записей: {DeletedCount}",
                result.UpdatedCount,
                result.DeletedCount);

            return Convert.ToInt32(result.DeletedCount);
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(exception, "Ошибка пакетной очистки подразделений");
            throw;
        }
    }

    private sealed class CleanupResult
    {
        public long UpdatedCount { get; init; }

        public long DeletedCount { get; init; }
    }
}