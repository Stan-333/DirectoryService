using System.Data;
using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace Shared.Core.Abstractions;

public interface ITransactionManager
{
    /// <summary>
    /// Начинает транзакцию с уровнем изоляции по умолчанию (<see cref="IsolationLevel.ReadCommitted"/>).
    /// </summary>
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken = default);
}