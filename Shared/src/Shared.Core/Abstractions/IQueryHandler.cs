using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace Shared.Core.Abstractions;

public interface IQueryHandler<TResponse, in TQuery>
    where TQuery : IQuery
{
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}

public interface IQueryHandlerWithResult<TResponse, in TQuery>
    where TQuery : IQuery
{
    Task<Result<TResponse, Errors>> Handle(TQuery query, CancellationToken cancellationToken);
}