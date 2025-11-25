using DirectoryService.Domain.Locations;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
     Task<Guid> AddAsync(Location location, CancellationToken cancellationToken = default);

     // Task<Guid> UpdateAsync(Location location, CancellationToken cancellationToken = default);

     // Task<Guid> DeleteAsync(LocationId id, CancellationToken cancellationToken = default);

     Task<int> GetCountByNameAsync(LocationName locationName, CancellationToken cancellationToken = default);

     Task<int> GetCountByAddressAsync(Address address, CancellationToken cancellationToken = default);
}