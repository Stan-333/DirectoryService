using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;

    public LocationsRepository(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> AddAsync(Location location, CancellationToken cancellationToken = default)
    {
        await _dbContext.Locations.AddAsync(location, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return location.Id.Value;
    }

    public async Task<int> GetCountByNameAsync(LocationName locationName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations.CountAsync(l => l.Name == locationName, cancellationToken);
    }

    public async Task<int> GetCountByAddressAsync(Address address, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations.CountAsync(l => l.Address == address, cancellationToken);
    }
}