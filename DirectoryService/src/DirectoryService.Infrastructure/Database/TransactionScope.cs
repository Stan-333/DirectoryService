using System.Data;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Infrastructure.Database;

public class TransactionScope : ITransactionScope
{
    private readonly IDbTransaction _transaction;
    private readonly ILogger<TransactionScope> _logger;

    public TransactionScope(IDbTransaction transaction, ILogger<TransactionScope> logger)
    {
        _transaction = transaction;
        _logger = logger;
    }

    public UnitResult<Error> Commit()
    {
        try
        {
            _transaction.Commit();
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка завершения транзакции");
            return Error.Failure("transaction.commit.failed", "Ошибка завершения транзакции");
        }
    }

    public UnitResult<Error> Rollback()
    {
        try
        {
            _transaction.Rollback();
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка отката транзакции");
            return Error.Failure("transaction.rollback.failed", "Ошибка отката транзакции");
        }
    }

    public void Dispose()
    {
        _transaction.Dispose();
    }
}