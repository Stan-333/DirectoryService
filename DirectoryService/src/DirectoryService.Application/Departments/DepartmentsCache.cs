using System.Globalization;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments;

public static class DepartmentsCache
{
    public const string Tag = "departments";

    public static readonly IReadOnlyList<string> Tags = new[] { Tag };

    public static string ChildrenKey(Guid parentId, GetChildrenDepartmentsRequest request)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"departments:children:{parentId:N}:p={request.Page}:ps={request.PageSize}");

    public static string RootsWithChildrenKey(GetRootDepartmentsWithChildrenRequest request)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"departments:roots:p={request.Page}:ps={request.PageSize}:pf={request.Prefetch}");

    public static string TopByPositionKey(GetTopDepartmentsByPositionsRequest request)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"departments:top-by-position:rc={request.RowsCount}");
}