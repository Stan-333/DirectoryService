using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    public string Value { get; }

    private DepartmentName(string value) => Value = value;

    public static Result<DepartmentName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("department name");
        if (value.Length is < LengthConstants.LENGTH3 or > LengthConstants.LENGTH150)
            return GeneralErrors.ValueIsInvalid("department name");
        return new DepartmentName(value);
    }
}