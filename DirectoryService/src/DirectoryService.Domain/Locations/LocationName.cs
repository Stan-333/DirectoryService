using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Locations;

public record LocationName
{
    public string Value { get; }

    private LocationName(string value) => Value = value;

    public static Result<LocationName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("location name");
        return value.Length is < LengthConstants.LENGTH3 or > LengthConstants.LENGTH120
            ? GeneralErrors.ValueIsInvalid("location name")
            : new LocationName(value);
    }
}