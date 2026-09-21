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

    public Errors(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        _errors = [..errors];
    }

    public int Count => _errors.Count;

    public Error this[int index] => _errors[index];

    public static implicit operator Errors(List<Error> errors) => new(errors);

    public static implicit operator Errors(Error[] errors) => new(errors);

    public static implicit operator Errors(Error error) => new([error]);

    public IEnumerator<Error> GetEnumerator()
    {
        return _errors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}