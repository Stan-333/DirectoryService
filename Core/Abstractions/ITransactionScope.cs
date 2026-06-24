using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace Core.Abstractions;

public interface ITransactionScope : IDisposable
{
    public UnitResult<Error> Commit();

    public UnitResult<Error> Rollback();
}