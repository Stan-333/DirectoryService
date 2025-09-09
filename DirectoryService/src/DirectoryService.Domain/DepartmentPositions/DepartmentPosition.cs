namespace DirectoryService.Domain.DepartmentPositions;

public class DepartmentPosition
{
    public Guid DepartmentId { get; private set; }

    public Guid PositionId { get; private set; }

    public DepartmentPosition(Guid departmentId, Guid positionId)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
    }
}