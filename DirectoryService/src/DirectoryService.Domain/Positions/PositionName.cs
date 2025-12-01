using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Positions;

public record PositionName
{
    public const int NAME_MAX_LENGTH = 100;
    private const int NAME_MIN_LENGTH = 3;

    public string Value { get; }

    private PositionName(string value) => Value = value;

    public static Result<PositionName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("position name");
        return value.Length is < NAME_MIN_LENGTH or > NAME_MAX_LENGTH
            ? GeneralErrors.ValueIsInvalid("position name")
            : new PositionName(value);
    }
}