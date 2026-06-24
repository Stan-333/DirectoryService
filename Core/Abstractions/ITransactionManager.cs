using System.Data;
using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace Core.Abstractions;

public interface ITransactionManager
{
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(
        CancellationToken cancellationToken = default,
        IsolationLevel? isolationLevel = null);

    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken = default);
}