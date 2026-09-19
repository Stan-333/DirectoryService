using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Infrastructure.Database;

public sealed class TransactionScope : ITransactionScope
{
    private readonly IDbContextTransaction _transaction;
    private readonly ILogger<TransactionScope> _logger;

    public TransactionScope(IDbContextTransaction transaction, ILogger<TransactionScope> logger)
    {
        _transaction = transaction;
        _logger = logger;
    }

    public async Task<UnitResult<Error>> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _transaction.CommitAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка завершения транзакции");
            return Error.Failure("transaction.commit.failed", "Ошибка завершения транзакции");
        }
    }

    public async Task<UnitResult<Error>> RollbackAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _transaction.RollbackAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка отката транзакции");
            return Error.Failure("transaction.rollback.failed", "Ошибка отката транзакции");
        }
    }

    public ValueTask DisposeAsync()
    {
        return _transaction.DisposeAsync();
    }
}