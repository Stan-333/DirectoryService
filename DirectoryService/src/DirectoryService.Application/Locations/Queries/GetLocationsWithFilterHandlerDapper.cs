using System.Data;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.Responses;

namespace DirectoryService.Application.Locations.Queries;

public class GetLocationsWithFilterHandlerDapper
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetLocationsWithFilterHandlerDapper(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<GetLocationsWithFiltersResponse> Handle(
        GetLocationsWithFiltersQuery query,
        CancellationToken cancellationToken)
    {
        var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        var conditions = new List<string>();
        string joinClause = string.Empty;

        if (!string.IsNullOrWhiteSpace(query.Request.Search))
        {
            conditions.Add("location_name ILIKE @search");
            parameters.Add("search", $"%{query.Request.Search}%", DbType.String);
        }

        if (query.Request.IsActive.HasValue)
        {
            conditions.Add("is_active = @is_active");
            parameters.Add("is_active", query.Request.IsActive, DbType.Boolean);
        }

        if (query.Request.DepartmentIds?.Any() == true)
        {
            joinClause = "JOIN department_locations ON locations.location_id = department_locations.location_id";
            conditions.Add("department_locations.department_id = ANY(@department_ids)");
            parameters.Add("department_ids", query.Request.DepartmentIds.ToArray());
        }

        parameters.Add("offset", (query.Request.Pagination.Page - 1) * query.Request.Pagination.PageSize, DbType.Int32);
        parameters.Add("page_size", query.Request.Pagination.PageSize, DbType.Int32);

        string whereClause = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions.ToArray())}" : string.Empty;
        string orderByField = query.Request.SortBy?.ToLower() switch
        {
            "location_name" => "locations.location_name",
            "created_at" => "locations.created_at",
            _ => "locations.location_name",
        };

        string orderByClause = query.Request.SortDirection == "asc"
            ? $"ORDER BY {orderByField} ASC"
            : $"ORDER BY {orderByField} DESC";

        long? totalCount = null;

        var locationsDto = await connection.QueryAsync<GetLocationDto, AddressDto, long, GetLocationDto>(
            $"""
            SELECT
              locations.location_id,
              locations.location_name,
              locations.is_active,
              locations.timezone,
              locations.created_at,
              locations.updated_at,
              locations.postal_code,
              locations.region,
              locations.city,
              locations.street,
              locations.house,
              locations.apartment,
              COUNT(*) OVER() as total_count
            FROM
              locations
            {joinClause}
            {whereClause}
            {orderByClause}
            LIMIT @page_size OFFSET @offset
            """,
            param: parameters,
            splitOn: "postal_code,total_count",
            map: (locationDto, addressDto, count) =>
            {
                locationDto.Address = addressDto;
                totalCount ??= count;
                return locationDto;
            });
        var locationsDtoList = locationsDto.ToList();
        return new GetLocationsWithFiltersResponse(locationsDtoList, totalCount ?? 0);
    }
}