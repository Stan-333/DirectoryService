using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using TimeZone = System.TimeZone;

namespace DirectoryService.IntegrationTests.Infrastructure;

// IClassFixture<T> - позволяет использовать фикстуру для всех тестов в классе.
// Это интерфейс, и он нужен для того, чтобы объединить несколько классов в один общий контекст, где мы говорим, что
// конструктор класса DirectoryTestWebFactory (от которого мы наследуемся в контексте IClassFixture<DirectoryTestWebFactory>)
// сработает всего один раз для всех тестов в этом классе
public class DirectoryBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;

    private int _entity_increment;

    protected IServiceProvider Services { get; set; }

    protected DirectoryBaseTests(DirectoryTestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
        _entity_increment = 0;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }

    protected async Task<T> ExecuteInDb<T>(Func<DirectoryServiceDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        return await action(dbContext);
    }

    protected async Task ExecuteInDb(Func<DirectoryServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        await action(dbContext);
    }

    protected async Task<LocationId> CreateLocationAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var location = Location.Create(
                LocationName.Create($"Локация-{++_entity_increment}").Value,
                Address.Create(
                    "000000",
                    "г. Москва",
                    "г. Москва",
                    "ул. Ленина",
                    "1",
                    _entity_increment.ToString()).Value,
                Domain.Locations.TimeZone.Create("Europe/Moscow").Value,
                true,
                DateTime.UtcNow);

            await dbContext.Locations.AddAsync(location.Value, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return location.Value.Id;
        });
    }

    protected async Task<List<LocationId>> CreateManyLocationsAsync(int count, CancellationToken cancellationToken = default)
    {
        return await ExecuteInDb(async dbContext =>
        {
            List<LocationId> locations = [];
            for (int i = 0; i < count; i++)
            {
                _entity_increment++;
                var location = Location.Create(
                    LocationName.Create($"Локация-{_entity_increment}").Value,
                    Address.Create(
                        "000000",
                        "г. Москва",
                        "г. Москва",
                        "ул. Ленина",
                        "1",
                        _entity_increment.ToString()).Value,
                    Domain.Locations.TimeZone.Create("Europe/Moscow").Value,
                    true,
                    DateTime.UtcNow);

                await dbContext.Locations.AddAsync(location.Value, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                locations.Add(location.Value.Id);
            }

            return locations;
        });
    }
}