using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace DirectoryService.IntegrationTests.Locations;

public class CreateLocationTests : DirectoryBaseTests
{
    public CreateLocationTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateLocation_with_valid_data_should_success()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await ExecuteHandler(sut =>
            sut.Handle(new CreateLocationCommand(BuildRequest("Локация-А", "1")), cancellationToken));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task CreateLocation_with_duplicate_name_should_return_conflict()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var first = await ExecuteHandler(sut =>
            sut.Handle(new CreateLocationCommand(BuildRequest("Дубликат имени", "1")), cancellationToken));
        Assert.True(first.IsSuccess);

        // Act — same name, different address
        var second = await ExecuteHandler(sut =>
            sut.Handle(new CreateLocationCommand(BuildRequest("Дубликат имени", "2")), cancellationToken));

        // Assert
        Assert.True(second.IsFailure);
        Assert.Contains(second.Error, e => e.Type == ErrorType.CONFLICT);
    }

    [Fact]
    public async Task CreateLocation_with_duplicate_address_should_return_conflict()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var first = await ExecuteHandler(sut =>
            sut.Handle(new CreateLocationCommand(BuildRequest("Имя-1", "10")), cancellationToken));
        Assert.True(first.IsSuccess);

        // Act — different name, same address
        var second = await ExecuteHandler(sut =>
            sut.Handle(new CreateLocationCommand(BuildRequest("Имя-2", "10")), cancellationToken));

        // Assert
        Assert.True(second.IsFailure);
        Assert.Contains(second.Error, e => e.Type == ErrorType.CONFLICT);
    }

    private static CreateLocationRequest BuildRequest(string name, string house) =>
        new(
            name,
            new AddressDto
            {
                PostalCode = "000000",
                Region = "г. Москва",
                City = "г. Москва",
                Street = "ул. Ленина",
                House = house,
                Apartment = "1",
            },
            "Europe/Moscow");

    private async Task<T> ExecuteHandler<T>(Func<CreateLocationHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<CreateLocationHandler>();

        return await action(sut);
    }
}