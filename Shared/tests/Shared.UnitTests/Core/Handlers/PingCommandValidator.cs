using FluentValidation;

namespace Shared.UnitTests.Core.Handlers;

public sealed class PingCommandValidator : AbstractValidator<PingCommand>
{
    public PingCommandValidator()
    {
        RuleFor(c => c.Text).NotEmpty();
    }
}