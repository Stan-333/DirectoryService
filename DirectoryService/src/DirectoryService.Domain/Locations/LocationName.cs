using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Locations;

public record LocationName
{
    public const int NAME_MAX_LENGTH = 120;
    private const int NAME_MIN_LENGTH = 3;

    public string Value { get; }

    private LocationName(string value) => Value = value;

    public static Result<LocationName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("location name");
        return value.Length is < NAME_MIN_LENGTH or > NAME_MAX_LENGTH
            ? GeneralErrors.ValueIsInvalid("location name")
            : new LocationName(value);
    }
}