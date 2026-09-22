namespace Shared.Kernel.Exceptions;

/// <summary>
/// Ожидаемая ошибка приложения там, где её неудобно вернуть через Result.
/// Несёт список <see cref="Errors"/>. HTTP-статус ответа определяется типами ошибок
/// (<see cref="ErrorType"/>), поэтому отдельный класс на каждый статус не нужен.
/// Класс не запечатан: сервис может завести своих наследников, например LocationNotFoundException.
/// </summary>
public class AppException : Exception
{
    public AppException(Error error, Exception? innerException = null)
        : this((error ?? throw new ArgumentNullException(nameof(error))).ToErrors(), innerException)
    {
    }

    public AppException(Errors errors, Exception? innerException = null)
        : base(BuildMessage(errors), innerException)
    {
        Errors = errors;
    }

    public Errors Errors { get; }

    private static string BuildMessage(Errors errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Count == 0)
        {
            throw new ArgumentException("Список ошибок не может быть пустым.", nameof(errors));
        }

        return string.Join("; ", errors.Select(e => $"{e.Code}: {e.Message}"));
    }
}