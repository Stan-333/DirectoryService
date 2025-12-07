using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Application.Abstractions;

public interface ITransactionScope : IDisposable
{
    public UnitResult<Error> Commit();

    public UnitResult<Error> Rollback();
}