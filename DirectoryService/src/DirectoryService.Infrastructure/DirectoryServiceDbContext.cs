using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure;

public class DirectoryServiceDbContext : DbContext, IReadDbContext
{
    public DirectoryServiceDbContext(
        DbContextOptions<DirectoryServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Location> Locations => Set<Location>();

    public DbSet<Position> Positions => Set<Position>();

    public DbSet<DepartmentLocation> DepartmentLocations => Set<DepartmentLocation>();

    public DbSet<DepartmentPosition> DepartmentPositions => Set<DepartmentPosition>();

    public IQueryable<Department> DepartmentsRead => Set<Department>().AsQueryable().AsNoTracking();

    public IQueryable<Location> LocationsRead => Set<Location>().AsQueryable().AsNoTracking();

    public IQueryable<Position> PositionsRead => Set<Position>().AsQueryable().AsNoTracking();

    public IQueryable<DepartmentPosition> DepartmentPositionsRead => Set<DepartmentPosition>().AsQueryable().AsNoTracking();

    public IQueryable<DepartmentLocation> DepartmentLocationsRead => Set<DepartmentLocation>().AsQueryable().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("ltree");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectoryServiceDbContext).Assembly);
    }
}