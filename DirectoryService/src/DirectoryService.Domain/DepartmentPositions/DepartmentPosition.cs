using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Domain.DepartmentPositions;

public class DepartmentPosition
{
    public DepartmentId DepartmentId { get; private set; }

    public PositionId PositionId { get; private set; }

    // Навигации заполняет EF Core при Include; null! — рекомендация EF Core для обязательных навигаций.
    public Department Department { get; private set; } = null!;

    public Position Position { get; private set; } = null!;

    public DepartmentPosition(DepartmentId departmentId, PositionId positionId)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    // EF Core: свойства заполняются при материализации из БД.
#pragma warning disable CS8618
    private DepartmentPosition() { }
#pragma warning restore CS8618
}