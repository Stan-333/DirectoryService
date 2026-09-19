using Stan333.Core.Abstractions;

namespace Stan333.Shared.UnitTests.Core.Handlers;

public sealed record PingCommand(string Text) : ICommand;