namespace Shared;

public static class GeneralErrors
{
    public static Error ValueIsInvalid(string? name = null)
    {
        string label = name ?? "Значение";
        return Error.Validation("value.is.invalid", $"{label} является недействительным");
    }

    public static Error ValueIsRequired(string? name = null)
    {
        string label = name is null ? string.Empty : $"'{name}' ";
        return Error.Validation("value.is.required", $"Поле {label}является обязательным");
    }

    public static Error ListHasDuplicates(string? listName = null)
    {
        string label = listName is null ? string.Empty : $"'{listName}' ";
        return Error.Validation("value.is.invalid", $"Список {label}имеет дубликаты");
    }

    public static Error AlreadyExist(string? recordName = null)
    {
        string label = recordName is null ? "Запись" : $"Такой '{recordName}'";
        return Error.Validation("record.already.exist", $"{label} уже существует");
    }

    public static Error NotFound(Guid? id = null, string? name = null)
    {
        string forId = id is null ? string.Empty : $" по Id '{id}'";
        return Error.NotFound("record.not.found", $"{name ?? "запись"} не найдена{forId}", id);
    }

    public static Error Failure(string? message = null)
    {
        return Error.Failure("server.failure", message ?? "Серверная ошибка");
    }
}