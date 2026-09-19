using Stan333.Core.Abstractions;

namespace Stan333.Shared.UnitTests.Core.Handlers;

public sealed class PingQueryHandler : IQueryHandler<int, PingQuery>
{
    public Task<int> Handle(PingQuery query, CancellationToken cancellationToken) => Task.FromResult(42);
}