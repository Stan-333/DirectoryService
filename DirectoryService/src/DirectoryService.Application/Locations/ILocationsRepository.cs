using DirectoryService.Domain.Locations;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
     Task<Guid> AddAsync(Location location, CancellationToken cancellationToken = default);

     // Task<Guid> UpdateAsync(Location location, CancellationToken cancellationToken = default);

     // Task<Guid> DeleteAsync(LocationId id, CancellationToken cancellationToken = default);

     // Task<Location> GetByIdAsync(LocationId id, CancellationToken cancellationToken = default);
}