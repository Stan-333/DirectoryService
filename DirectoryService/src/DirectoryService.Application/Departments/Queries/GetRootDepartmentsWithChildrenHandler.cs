using System.Data;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.Responses;

namespace DirectoryService.Application.Departments.Queries;

public class GetRootDepartmentsWithChildrenHandler
    : IQueryHandler<GetRootDepartmentsWithChildrenResponse, GetRootDepartmentsWithChildrenQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetRootDepartmentsWithChildrenHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<GetRootDepartmentsWithChildrenResponse> Handle(
        GetRootDepartmentsWithChildrenQuery query,
        CancellationToken cancellationToken)
    {
        var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new DynamicParameters();
        parameters.Add("child_limit", query.Request.Prefetch);
        parameters.Add("offset", (query.Request.Page - 1) * query.Request.PageSize, DbType.Int32);
        parameters.Add("root_limit", query.Request.PageSize, DbType.Int32);

        var departmentsDto = await connection.QueryAsync<DepartmentWithChildrenInfoDto>(
            """
            WITH roots AS (SELECT d.department_id,
                                  d.parent_id,
                                  d.department_name,
                                  d.identifier,
                                  d.path,
                                  d.depth,
                                  d.is_active,
                                  d.created_at,
                                  d.updated_at
                           FROM departments d
                           WHERE d.parent_id IS NULL
                           ORDER BY d.created_at
                           OFFSET @offset LIMIT @root_limit
                           )
            -- получаем родительские подразделения
            SELECT *,
                   (EXISTS(SELECT 1 FROM departments WHERE parent_id = roots.department_id OFFSET @child_limit LIMIT 1)) AS has_more_children
            FROM roots
            
            UNION ALL
            
            -- получаем дочерние подразделения
            SELECT c.*,
                   (EXISTS(SELECT 1 FROM departments WHERE parent_id = c.department_id)) AS has_more_children
            FROM roots r CROSS JOIN LATERAL (
                SELECT d.department_id,
                       d.parent_id,
                       d.department_name,
                       d.identifier,
                       d.path,
                       d.depth,
                       d.is_active,
                       d.created_at,
                       d.updated_at
                FROM departments d
                WHERE d.parent_id = r.department_id
                  AND d.is_active = true
                ORDER BY d.created_at
                LIMIT @child_limit
                ) c;
            """,
            param: parameters);

        return new GetRootDepartmentsWithChildrenResponse(departmentsDto.ToList());
    }
}