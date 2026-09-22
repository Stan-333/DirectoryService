using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace Shared.Core.Abstractions;

public interface ITransactionScope : IAsyncDisposable
{
    Task<UnitResult<Error>> CommitAsync(CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> RollbackAsync(CancellationToken cancellationToken = default);
}