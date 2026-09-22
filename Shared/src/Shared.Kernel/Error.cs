using System.Text.Json.Serialization;

namespace Shared.Kernel;

/// <summary>
/// Ожидаемая ошибка: код для программ, сообщение для человека и тип, по которому выбирается HTTP-статус.
/// Создаётся фабричными методами по типу ошибки, например <see cref="NotFound"/> или <see cref="Validation"/>.
/// </summary>
public record Error
{
    /// <summary>
    /// Машиночитаемый код ошибки, например <c>location.not.found</c>. Клиенты сравнивают ошибки по нему.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Сообщение об ошибке для человека.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Тип ошибки. По нему определяется HTTP-статус ответа.
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// Поле запроса, которое не прошло проверку. Заполняется только у ошибок валидации.
    /// </summary>
    public string? InvalidField { get; }

    [JsonConstructor]
    private Error(string code, string message, ErrorType type, string? invalidField = null)
    {
        Code = code;
        Message = message;
        Type = type;
        InvalidField = invalidField;
    }

    /// <summary>
    /// Запись не найдена.
    /// </summary>
    /// <param name="code">Код ошибки; если <see langword="null"/>, то <c>record.not.found</c>.</param>
    /// <param name="message">Сообщение для человека.</param>
    /// <returns>Ошибка типа <see cref="ErrorType.NotFound"/>.</returns>
    public static Error NotFound(string? code, string message)
        => new(code ?? "record.not.found", message, ErrorType.NotFound);

    /// <summary>
    /// Входные данные не прошли проверку.
    /// </summary>
    /// <param name="code">Код ошибки; если <see langword="null"/>, то <c>value.is.invalid</c>.</param>
    /// <param name="message">Сообщение для человека.</param>
    /// <param name="invalidField">Поле запроса, которое не прошло проверку.</param>
    /// <returns>Ошибка типа <see cref="ErrorType.Validation"/>.</returns>
    public static Error Validation(string? code, string message, string? invalidField = null)
        => new(code ?? "value.is.invalid", message, ErrorType.Validation, invalidField);

    /// <summary>
    /// Операция противоречит текущему состоянию, например запись с таким именем уже есть.
    /// </summary>
    /// <param name="code">Код ошибки; если <see langword="null"/>, то <c>value.is.conflict</c>.</param>
    /// <param name="message">Сообщение для человека.</param>
    /// <returns>Ошибка типа <see cref="ErrorType.Conflict"/>.</returns>
    public static Error Conflict(string? code, string message)
        => new(code ?? "value.is.conflict", message, ErrorType.Conflict);

    /// <summary>
    /// Сбой на стороне сервиса, клиент исправить его не может.
    /// </summary>
    /// <param name="code">Код ошибки; если <see langword="null"/>, то <c>failure</c>.</param>
    /// <param name="message">Сообщение для человека. Подробности сбоя в него не включают, они уходят в лог.</param>
    /// <returns>Ошибка типа <see cref="ErrorType.Failure"/>.</returns>
    public static Error Failure(string? code, string message)
        => new(code ?? "failure", message, ErrorType.Failure);

    /// <summary>
    /// Пользователь не аутентифицирован.
    /// </summary>
    /// <param name="code">Код ошибки; если <see langword="null"/>, то <c>authentication</c>.</param>
    /// <param name="message">Сообщение для человека.</param>
    /// <returns>Ошибка типа <see cref="ErrorType.Authentication"/>.</returns>
    public static Error Authentication(string? code, string message)
        => new(code ?? "authentication", message, ErrorType.Authentication);

    /// <summary>
    /// У пользователя нет прав на операцию.
    /// </summary>
    /// <param name="code">Код ошибки; если <see langword="null"/>, то <c>authorization</c>.</param>
    /// <param name="message">Сообщение для человека.</param>
    /// <returns>Ошибка типа <see cref="ErrorType.Authorization"/>.</returns>
    public static Error Authorization(string? code, string message)
        => new(code ?? "authorization", message, ErrorType.Authorization);

    /// <summary>
    /// Оборачивает ошибку в список из одного элемента, например для <c>Result&lt;T, Errors&gt;</c>.
    /// </summary>
    /// <returns>Список с этой ошибкой.</returns>
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