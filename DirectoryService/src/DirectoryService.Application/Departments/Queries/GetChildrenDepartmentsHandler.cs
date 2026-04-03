using System.Data;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.Responses;

namespace DirectoryService.Application.Departments.Queries;

public class GetChildrenDepartmentsHandler : IQueryHandler<GetChildrenDepartmentsResponse, GetChildrenDepartmentsQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetChildrenDepartmentsHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<GetChildrenDepartmentsResponse> Handle(
        GetChildrenDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new DynamicParameters();
        parameters.Add("parentId", query.ParentId, DbType.Guid);
        parameters.Add("offset", (query.Request.Page - 1) * query.Request.PageSize, DbType.Int32);
        parameters.Add("child_limit", query.Request.PageSize, DbType.Int32);

        var departmentsDto = await connection.QueryAsync<DepartmentWithChildrenInfoDto>(
            """
            WITH children AS (
            SELECT department_id,
                   parent_id,
                   department_name,
                   identifier,
                   path,
                   depth,
                   is_active,
                   created_at,
                   updated_at
            FROM departments
            WHERE parent_id = @parentId
            ORDER BY created_at
            OFFSET @offset LIMIT @child_limit)
            SELECT *,
                   (EXISTS(SELECT 1 FROM departments WHERE parent_id = children.department_id)) AS has_more_children
            FROM children;
            """,
            param: parameters);

        return new GetChildrenDepartmentsResponse(departmentsDto.ToList());
    }
}