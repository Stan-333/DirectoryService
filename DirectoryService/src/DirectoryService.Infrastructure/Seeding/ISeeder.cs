namespace DirectoryService.Infrastructure.Seeding;

public interface ISeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}