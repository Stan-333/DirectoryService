namespace Shared;

public static class GeneralErrors
{
    public static Error ValueIsInvalid(string? name = null)
    {
        string label = name ?? "значение";
        return Error.Validation("value.is.invalid", $"{label} является недействительным");
    }

    public static Error NotFound(Guid? id = null, string? name = null)
    {
        string forId = id is null ? string.Empty : $" по Id '{id}'";
        return Error.NotFound("record.not.found", $"{name ?? "запись"} не найдена{forId}", id);
    }

    public static Error ValueIsRequired(string? name = null)
    {
        string label = name is null ? string.Empty : $" {name} ";
        return Error.Validation("value.is.required", $"Поле {label} является обязательным");
    }

    public static Error AlreadyExist()
    {
        return Error.Validation("record.already.exist", "Запись уже существует");
    }

    public static Error Failure(string? message = null)
    {
        return Error.Failure("server.failure", message ?? "Серверная ошибка");
    }
}