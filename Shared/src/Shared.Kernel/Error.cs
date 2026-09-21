using System.Text.Json.Serialization;

namespace Shared.Kernel;

public record Error
{
    public string Code { get; }

    public string Message { get; }

    public ErrorType Type { get; }

    public string? InvalidField { get; }

    [JsonConstructor]
    private Error(string code, string message, ErrorType type, string? invalidField = null)
    {
        Code = code;
        Message = message;
        Type = type;
        InvalidField = invalidField;
    }

    public static Error NotFound(string? code, string message)
        => new(code ?? "record.not.found", message, ErrorType.NotFound);

    public static Error Validation(string? code, string message, string? invalidField = null)
        => new(code ?? "value.is.invalid", message, ErrorType.Validation, invalidField);

    public static Error Conflict(string? code, string message)
        => new(code ?? "value.is.conflict", message, ErrorType.Conflict);

    public static Error Failure(string? code, string message)
        => new(code ?? "failure", message, ErrorType.Failure);

    public static Error Authentication(string? code, string message)
        => new(code ?? "authentication", message, ErrorType.Authentication);

    public static Error Authorization(string? code, string message)
        => new(code ?? "authorization", message, ErrorType.Authorization);

    public Errors ToErrors() => new([this]);
}

/// <summary>
/// Тип ошибки. В JSON передаётся строкой (<c>"NotFound"</c>), а не числом,
/// поэтому порядок членов можно менять без поломки контракта.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ErrorType>))]
public enum ErrorType
{
    /// <summary>
    /// Ошибка с валидацией.
    /// </summary>
    Validation,

    /// <summary>
    /// Ошибка ничего не найдено.
    /// </summary>
    NotFound,

    /// <summary>
    /// Ошибка сервера.
    /// </summary>
    Failure,

    /// <summary>
    /// Ошибка конфликт.
    /// </summary>
    Conflict,

    /// <summary>
    /// Ошибка аутентификации.
    /// </summary>
    Authentication,

    /// <summary>
    /// Ошибка авторизации.
    /// </summary>
    Authorization,
}