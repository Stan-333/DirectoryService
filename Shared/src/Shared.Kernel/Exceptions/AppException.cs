namespace Shared.Kernel.Exceptions;

/// <summary>
/// Ожидаемая ошибка приложения там, где её неудобно вернуть через Result.
/// Несёт список <see cref="Errors"/>. HTTP-статус ответа определяется типами ошибок
/// (<see cref="ErrorType"/>), поэтому отдельный класс на каждый статус не нужен.
/// Класс не запечатан: сервис может завести своих наследников, например LocationNotFoundException.
/// </summary>
public class AppException : Exception
{
    /// <summary>
    /// Создаёт исключение с одной ошибкой.
    /// </summary>
    /// <param name="error">Ошибка.</param>
    /// <param name="innerException">Исключение, из-за которого возникла ошибка.</param>
    public AppException(Error error, Exception? innerException = null)
        : this((error ?? throw new ArgumentNullException(nameof(error))).ToErrors(), innerException)
    {
    }

    /// <summary>
    /// Создаёт исключение со списком ошибок. Список не может быть пустым.
    /// </summary>
    /// <param name="errors">Ошибки.</param>
    /// <param name="innerException">Исключение, из-за которого возникли ошибки.</param>
    public AppException(Errors errors, Exception? innerException = null)
        : base(BuildMessage(errors), innerException)
    {
        Errors = errors;
    }

    /// <summary>
    /// Ошибки, которые вернутся клиенту.
    /// </summary>
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