using System.Linq.Expressions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Locations.Queries;

public class GetLocationsWithFiltersHandler : IQueryHandler<GetLocationsWithFiltersResponse, GetLocationsWithFiltersQuery>
{
    private readonly IReadDbContext _readDbContext;

    public GetLocationsWithFiltersHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<GetLocationsWithFiltersResponse> Handle(
        GetLocationsWithFiltersQuery query,
        CancellationToken cancellationToken)
    {
        var locationsQuery = _readDbContext.LocationsRead;

        if (!string.IsNullOrWhiteSpace(query.Request.Search))
        {
            locationsQuery = locationsQuery
                .Where(l => EF.Functions.Like(
                    l.Name.Value.ToLower(), $"%{query.Request.Search.ToLower()}%"));
        }

        if (query.Request.IsActive.HasValue)
            locationsQuery = locationsQuery.Where(l => l.IsActive == query.Request.IsActive);

        if (query.Request.DepartmentIds != null && query.Request.DepartmentIds.Any())
        {
            var departmentIds = query.Request.DepartmentIds
                .Select(id => new DepartmentId(id))
                .ToList();

            locationsQuery = locationsQuery.Where(l => l.DepartmentLocations
                .Any(dl => departmentIds.Contains(dl.DepartmentId)));
        }

        Expression<Func<Location, object>> keySelector = query.Request.SortBy?.ToLower() switch
        {
            "location_name" => l => l.Name.Value,
            "created_at" => l => l.CreatedAt,
            _ => l => l.Name.Value,
        };

        locationsQuery = query.Request.SortDirection == "asc"
            ? locationsQuery.OrderBy(keySelector)
            : locationsQuery.OrderByDescending(keySelector);

        int totalCount = await locationsQuery.CountAsync(cancellationToken);

        locationsQuery = locationsQuery
            .Skip((query.Request.Pagination.Page - 1) * query.Request.Pagination.PageSize)
            .Take(query.Request.Pagination.PageSize);

        var locations = await locationsQuery
            .Select(l => new GetLocationDto
            {
                LocationId = l.Id.Value,
                LocationName = l.Name.Value,
                Address = new AddressDto
                {
                    PostalCode = l.Address.PostalCode,
                    Region = l.Address.Region,
                    City = l.Address.City,
                    Street = l.Address.Street,
                    House = l.Address.House,
                    Apartment = l.Address.Apartment,
                },
                TimeZone = l.Timezone.Value,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        return new GetLocationsWithFiltersResponse(locations, totalCount);
    }
}