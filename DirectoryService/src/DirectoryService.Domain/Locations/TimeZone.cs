using CSharpFunctionalExtensions;
using Shared;
using TimeZoneConverter;

namespace DirectoryService.Domain.Locations;

public record TimeZone
{
    public string Value { get; }

    private TimeZone(string value) => Value = value;

    public static Result<TimeZone, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("time zone");
        return TZConvert.KnownIanaTimeZoneNames.Contains(value)
            ? new TimeZone(value)
            : GeneralErrors.ValueIsInvalid("time zone");
    }
}