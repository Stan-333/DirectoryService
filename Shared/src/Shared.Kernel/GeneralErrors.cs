namespace Shared.Kernel;

/// <summary>
/// Типовые ошибки с готовыми кодами и сообщениями. Ошибки конкретного домена
/// (например, <c>location.not.found</c>) сервис описывает сам через фабрики <see cref="Error"/>.
/// </summary>
public static class GeneralErrors
{
    /// <summary>
    /// Значение недопустимо. Код <c>value.is.invalid</c>.
    /// </summary>
    /// <param name="name">Название значения для сообщения.</param>
    /// <returns>Ошибка валидации.</returns>
    public static Error ValueIsInvalid(string? name = null)
    {
        string label = name ?? "Значение";
        return Error.Validation("value.is.invalid", $"{label} является недействительным");
    }

    /// <summary>
    /// Обязательное поле не заполнено. Код <c>value.is.required</c>.
    /// </summary>
    /// <param name="name">Название поля для сообщения.</param>
    /// <returns>Ошибка валидации.</returns>
    public static Error ValueIsRequired(string? name = null)
    {
        string label = name is null ? string.Empty : $"'{name}' ";
        return Error.Validation("value.is.required", $"Поле {label}является обязательным");
    }

    /// <summary>
    /// В списке есть повторяющиеся элементы. Код <c>list.has.duplicates</c>.
    /// </summary>
    /// <param name="listName">Название списка для сообщения.</param>
    /// <returns>Ошибка валидации.</returns>
    public static Error ListHasDuplicates(string? listName = null)
    {
        string label = listName is null ? string.Empty : $"'{listName}' ";
        return Error.Validation("list.has.duplicates", $"Список {label}имеет дубликаты");
    }

    /// <summary>
    /// Такая запись уже существует. Код <c>record.already.exist</c>.
    /// </summary>
    /// <param name="recordName">Название записи для сообщения.</param>
    /// <returns>Ошибка конфликта.</returns>
    public static Error AlreadyExist(string? recordName = null)
    {
        string label = recordName is null ? "Запись" : $"Такой '{recordName}'";
        return Error.Conflict("record.already.exist", $"{label} уже существует");
    }

    /// <summary>
    /// Запись не найдена. Код <c>record.not.found</c>.
    /// </summary>
    /// <param name="id">Идентификатор записи для сообщения.</param>
    /// <param name="name">Название записи для сообщения.</param>
    /// <returns>Ошибка «не найдено».</returns>
    public static Error NotFound(Guid? id = null, string? name = null)
    {
        string forId = id is null ? string.Empty : $" по Id '{id}'";
        return Error.NotFound("record.not.found", $"{name ?? "запись"} не найдена{forId}");
    }

    /// <summary>
    /// Сбой на стороне сервиса. Код <c>server.failure</c>.
    /// </summary>
    /// <param name="message">Сообщение; по умолчанию «Серверная ошибка». Подробности сбоя в него не включают.</param>
    /// <returns>Ошибка типа <see cref="ErrorType.Failure"/>.</returns>
    public static Error Failure(string? message = null)
    {
        return Error.Failure("server.failure", message ?? "Серверная ошибка");
    }
}