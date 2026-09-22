using System.Collections;
using System.Text.Json.Serialization;

namespace Shared.Kernel;

/// <summary>
/// Список ошибок, доступный только для чтения. В JSON передаётся массивом <see cref="Error"/>.
/// </summary>
[JsonConverter(typeof(ErrorsJsonConverter))]
public class Errors : IReadOnlyList<Error>
{
    private readonly List<Error> _errors;

    /// <summary>
    /// Создаёт список из переданных ошибок. Последовательность копируется,
    /// поэтому дальнейшие изменения исходной коллекции на список не влияют.
    /// </summary>
    /// <param name="errors">Ошибки.</param>
    public Errors(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        _errors = [..errors];
    }

    /// <summary>
    /// Количество ошибок.
    /// </summary>
    public int Count => _errors.Count;

    /// <summary>
    /// Ошибка по индексу.
    /// </summary>
    /// <param name="index">Индекс, начиная с 0.</param>
    public Error this[int index] => _errors[index];

    /// <summary>
    /// Позволяет вернуть <see cref="List{T}"/> ошибок там, где ожидается <see cref="Errors"/>.
    /// </summary>
    /// <param name="errors">Ошибки.</param>
    public static implicit operator Errors(List<Error> errors) => new(errors);

    /// <summary>
    /// Позволяет вернуть массив ошибок там, где ожидается <see cref="Errors"/>.
    /// </summary>
    /// <param name="errors">Ошибки.</param>
    public static implicit operator Errors(Error[] errors) => new(errors);

    /// <summary>
    /// Позволяет вернуть одну ошибку там, где ожидается <see cref="Errors"/>.
    /// </summary>
    /// <param name="error">Ошибка.</param>
    public static implicit operator Errors(Error error) => new([error]);

    /// <inheritdoc/>
    public IEnumerator<Error> GetEnumerator()
    {
        return _errors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}