using Shared.Core.Abstractions;

namespace Shared.UnitTests.Core.Handlers;

public sealed record PingCommand(string Text) : ICommand;