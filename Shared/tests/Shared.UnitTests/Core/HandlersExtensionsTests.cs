using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Abstractions;
using Shared.Core.DependencyInjection;
using Shared.UnitTests.Core.Handlers;

namespace Shared.UnitTests.Core;

public class HandlersExtensionsTests
{
    [Fact]
    public async Task AddHandlers_registers_command_handler_by_interface_and_by_type_in_one_scope()
    {
        var services = new ServiceCollection();
        services.AddHandlers(typeof(PingCommandHandler).Assembly);
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        using IServiceScope scope = provider.CreateScope();

        var byInterface = scope.ServiceProvider.GetRequiredService<ICommandHandler<string, PingCommand>>();
        var byType = scope.ServiceProvider.GetRequiredService<PingCommandHandler>();

        byInterface.Should().BeSameAs(byType);
        (await byInterface.Handle(new PingCommand("pong"), CancellationToken.None)).Value.Should().Be("pong");
    }

    [Fact]
    public async Task AddHandlers_registers_query_handler()
    {
        var services = new ServiceCollection();
        services.AddHandlers(typeof(PingQueryHandler).Assembly);
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        using IServiceScope scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<int, PingQuery>>();

        (await handler.Handle(new PingQuery(), CancellationToken.None)).Should().Be(42);
    }

    [Fact]
    public void AddHandlers_uses_scoped_lifetime()
    {
        var services = new ServiceCollection();

        services.AddHandlers(typeof(PingCommandHandler).Assembly);

        services.Where(d => d.ServiceType == typeof(PingCommandHandler) || d.ServiceType == typeof(ICommandHandler<string, PingCommand>))
            .Should().NotBeEmpty()
            .And.OnlyContain(d => d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddHandlersAndValidators_registers_validators_too()
    {
        var services = new ServiceCollection();
        services.AddHandlersAndValidators(typeof(PingCommandHandler).Assembly);
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        using IServiceScope scope = provider.CreateScope();

        var validator = scope.ServiceProvider.GetRequiredService<IValidator<PingCommand>>();

        validator.Validate(new PingCommand(string.Empty)).IsValid.Should().BeFalse();
        scope.ServiceProvider.GetRequiredService<ICommandHandler<string, PingCommand>>().Should().NotBeNull();
    }
}