using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Positions;

public record PositionName
{
    public string Value { get; }

    private PositionName(string value) => Value = value;

    public static Result<PositionName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("position name");
        return value.Length is < LengthConstants.LENGTH3 or > LengthConstants.LENGTH100
            ? GeneralErrors.ValueIsInvalid("position name")
            : new PositionName(value);
    }
}