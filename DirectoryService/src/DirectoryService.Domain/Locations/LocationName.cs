using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Locations;

public record LocationName
{
    public string Value { get; }

    private LocationName(string value) => Value = value;

    public static Result<LocationName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 3 || value.Length > 120)
            return Result<LocationName>.Failure("Invalid name");
        return Result<LocationName>.Success(new LocationName(value));
    }
}