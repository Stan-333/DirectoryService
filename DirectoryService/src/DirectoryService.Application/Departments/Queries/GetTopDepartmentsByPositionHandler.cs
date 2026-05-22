using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.Responses;

namespace DirectoryService.Application.Departments.Queries;

public class GetTopDepartmentsByPositionHandler
    : IQueryHandler<GetTopDepartmentsByPositionResponse, GetTopDepartmentsByPositionQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ICacheService _cacheService;

    public GetTopDepartmentsByPositionHandler(
        IDbConnectionFactory dbConnectionFactory,
        ICacheService cacheService)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _cacheService = cacheService;
    }

    public async Task<GetTopDepartmentsByPositionResponse> Handle(
        GetTopDepartmentsByPositionQuery query,
        CancellationToken cancellationToken)
    {
        string cacheKey = DepartmentsCache.TopByPositionKey(query.Request);

        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            ct => LoadAsync(query, ct),
            tags: DepartmentsCache.Tags,
            cancellationToken: cancellationToken);
    }

    private async ValueTask<GetTopDepartmentsByPositionResponse> LoadAsync(
        GetTopDepartmentsByPositionQuery query,
        CancellationToken cancellationToken)
    {
        var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new DynamicParameters();
        parameters.Add("RowsCount", query.Request.RowsCount);
        var departmentsDto = await connection.QueryAsync<DepartmentWithPositionCountDto>(
            $"""
            WITH top_five AS (
                SELECT
                    department_positions.department_id,
                    COUNT(*) AS position_count
                FROM department_positions
                    INNER JOIN positions ON department_positions.position_id = positions.position_id
                    INNER JOIN departments ON department_positions.department_id = departments.department_id
                WHERE positions.is_active = true
                  AND departments.is_active = true
                GROUP BY department_positions.department_id
                ORDER BY position_count DESC
                LIMIT @RowsCount
            )
            SELECT
                departments.department_id,
                departments.department_name,
                departments."path",
                departments.created_at,
                top_five.position_count
            FROM top_five
                INNER JOIN departments ON top_five.department_id = departments.department_id
            WHERE departments.is_active = true
            ORDER BY top_five.position_count DESC, departments.department_name;
            """,
            param: parameters);
        return new GetTopDepartmentsByPositionResponse(departmentsDto.ToList());
    }
}