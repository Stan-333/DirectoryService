using CSharpFunctionalExtensions;
using Stan333.SharedKernel;

namespace Stan333.Core.Abstractions;

public interface ITransactionScope : IAsyncDisposable
{
    Task<UnitResult<Error>> CommitAsync(CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> RollbackAsync(CancellationToken cancellationToken = default);
}