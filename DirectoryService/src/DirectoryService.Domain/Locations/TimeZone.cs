using DirectoryService.Domain.Shared;
using TimeZoneConverter;

namespace DirectoryService.Domain.Locations;

public record TimeZone
{
    public string Value { get; }

    private TimeZone(string value) => Value = value;

    public static Result<TimeZone> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            Result<TimeZone>.Failure("Значение не может быть пустым");
        return TZConvert.KnownIanaTimeZoneNames.Contains(value)
            ? Result<TimeZone>.Success(new TimeZone(value))
            : Result<TimeZone>.Failure("Значение не соответствует формату IANA");
    }
}