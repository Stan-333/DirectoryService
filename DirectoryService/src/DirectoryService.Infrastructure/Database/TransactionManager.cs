using System.Data;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shared.Core.Abstractions;
using Shared.Kernel;

namespace DirectoryService.Infrastructure.Database;

public class TransactionManager : ITransactionManager
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<TransactionManager> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public TransactionManager(
        DirectoryServiceDbContext dbContext,
        ILogger<TransactionManager> logger,
        ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

    public async Task<Result<ITransactionScope, Error>> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IDbContextTransaction transaction = await _dbContext.Database.
                BeginTransactionAsync(isolationLevel, cancellationToken);
            ILogger<TransactionScope> transactionScopeLogger = _loggerFactory.CreateLogger<TransactionScope>();
            var transactionScope = new TransactionScope(transaction, transactionScopeLogger);

            return transactionScope;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка начала транзакции");
            return GeneralErrors.Failure();
        }
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
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
                    "Конфликт уникальности при сохранении транзакции. Ограничение: {ConstraintName}",
                    constraintName);
                return error;
            }

            _logger.LogError(ex, "Ошибка сохранения изменений");
            return GeneralErrors.Failure("Ошибка сохранения изменений");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка сохранения изменений");
            return GeneralErrors.Failure("Ошибка сохранения изменений");
        }
    }
}