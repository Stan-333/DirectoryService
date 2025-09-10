using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    public string Value { get; }

    private DepartmentName(string value) => Value = value;

    public static Result<DepartmentName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 3 || value.Length > 150)
            return Result<DepartmentName>.Failure("Invalid name");
        return Result<DepartmentName>.Success(new DepartmentName(value));
    }
}