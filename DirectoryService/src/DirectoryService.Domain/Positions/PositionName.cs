using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Positions;

public record PositionName
{
    public string Value { get; }

    private PositionName(string value) => Value = value;

    public static Result<PositionName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 3 || value.Length > 100)
            return Result<PositionName>.Failure("Invalid name");
        return Result<PositionName>.Success(new PositionName(value));
    }
}