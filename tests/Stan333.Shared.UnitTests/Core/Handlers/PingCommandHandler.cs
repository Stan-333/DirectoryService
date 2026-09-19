using CSharpFunctionalExtensions;
using Stan333.Core.Abstractions;
using Stan333.SharedKernel;

namespace Stan333.Shared.UnitTests.Core.Handlers;

public sealed class PingCommandHandler : ICommandHandler<string, PingCommand>
{
    public Task<Result<string, Errors>> Handle(PingCommand command, CancellationToken cancellationToken) =>
        Task.FromResult(Result.Success<string, Errors>(command.Text));
}