using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Departments;

public sealed record Identifier
{
    public const int MAX_LENGTH = 150;
    private const int MIN_LENGTH = 1;

    public string Value { get; }

    private Identifier(string value)
    {
        Value = value;
    }

    public static Result<Identifier, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("identifier");
        if (!Regex.IsMatch(value, $"^[a-zA-Z]{{{MIN_LENGTH},{MAX_LENGTH}}}$"))
            return GeneralErrors.ValueIsInvalid("identifier");
        return new Identifier(value);
    }
}