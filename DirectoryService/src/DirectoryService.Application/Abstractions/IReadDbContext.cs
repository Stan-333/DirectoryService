using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Application.Abstractions;

public interface IReadDbContext
{
    IQueryable<Department> DepartmentsRead { get; }

    IQueryable<Location> LocationsRead { get; }

    IQueryable<Position> PositionsRead { get; }

    IQueryable<DepartmentLocation> DepartmentLocationsRead { get; }

    IQueryable<DepartmentPosition> DepartmentPositionsRead { get; }
}