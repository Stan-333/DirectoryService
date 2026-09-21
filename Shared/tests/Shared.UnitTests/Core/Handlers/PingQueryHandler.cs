using Shared.Core.Abstractions;

namespace Shared.UnitTests.Core.Handlers;

public sealed class PingQueryHandler : IQueryHandler<int, PingQuery>
{
    public Task<int> Handle(PingQuery query, CancellationToken cancellationToken) => Task.FromResult(42);
}