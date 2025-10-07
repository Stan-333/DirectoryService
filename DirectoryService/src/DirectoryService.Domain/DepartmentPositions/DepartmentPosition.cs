using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Domain.DepartmentPositions;

public class DepartmentPosition
{
    public DepartmentId DepartmentId { get; private set; }

    public PositionId PositionId { get; private set; }

    public Department Department { get; private set; }

    public Position Position { get; private set; }

    // EF Core
    private DepartmentPosition() { }

    public DepartmentPosition(DepartmentId departmentId, PositionId positionId)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
    }
}