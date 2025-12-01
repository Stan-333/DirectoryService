using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    public const int NAME_MAX_LENGTH = 150;
    private const int NAME_MIN_LENGTH = 3;

    public string Value { get; }

    private DepartmentName(string value) => Value = value;

    public static Result<DepartmentName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("department name");
        if (value.Length is < NAME_MIN_LENGTH or > NAME_MAX_LENGTH)
            return GeneralErrors.ValueIsInvalid("department name");
        return new DepartmentName(value);
    }
}