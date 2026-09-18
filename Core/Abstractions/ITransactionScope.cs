using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace Core.Abstractions;

public interface ITransactionScope : IAsyncDisposable
{
    Task<UnitResult<Error>> CommitAsync(CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> RollbackAsync(CancellationToken cancellationToken = default);
}