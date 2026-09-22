using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shared.Kernel;
using TimeZone = DirectoryService.Domain.Locations.TimeZone;

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
        Assert.Contains(second.Error, e => e.Type == ErrorType.Conflict);
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
        Assert.Contains(second.Error, e => e.Type == ErrorType.Conflict);
    }

    [Fact]
    public async Task SaveLocation_with_duplicate_address_and_null_apartment_should_return_conflict()
    {
        // Arrange
        UnitResult<Errors> first = await SaveLocationDirectly(
            "Локация без квартиры 1",
            "20",
            null);
        Assert.True(first.IsSuccess);

        // Act
        UnitResult<Errors> second = await SaveLocationDirectly(
            "Локация без квартиры 2",
            "20",
            null);

        // Assert
        Assert.True(second.IsFailure);
        Assert.Contains(
            second.Error,
            error => error.Type == ErrorType.Conflict
                     && error.Message == "Локация с таким адресом уже существует");
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

    private async Task<UnitResult<Errors>> SaveLocationDirectly(
        string name,
        string house,
        string? apartment)
    {
        await using var scope = Services.CreateAsyncScope();
        ILocationsRepository repository = scope.ServiceProvider.GetRequiredService<ILocationsRepository>();

        Location location = Location.Create(
            LocationName.Create(name).Value,
            Address.Create(
                "000000",
                "г. Москва",
                "г. Москва",
                "ул. Ленина",
                house,
                apartment).Value,
            TimeZone.Create("Europe/Moscow").Value,
            true,
            DateTime.UtcNow).Value;

        await repository.AddAsync(location);
        return await repository.SaveChangesAsync();
    }
}