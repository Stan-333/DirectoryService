using CSharpFunctionalExtensions;
using Shared.Core.Abstractions;
using Shared.Kernel;

namespace Shared.UnitTests.Core.Handlers;

public sealed class PingCommandHandler : ICommandHandler<string, PingCommand>
{
    public Task<Result<string, Errors>> Handle(PingCommand command, CancellationToken cancellationToken) =>
        Task.FromResult(Result.Success<string, Errors>(command.Text));
}