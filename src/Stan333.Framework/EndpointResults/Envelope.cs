using System.Text.Json.Serialization;
using Stan333.SharedKernel;

namespace Stan333.Framework.EndpointResults;

public record Envelope
{
    public object? Result { get; }

    public Errors? ErrorList { get; }

    public bool IsError => ErrorList is { Count: > 0 };

    public DateTime TimeGenerated { get; }

    // timeGenerated принимается конструктором, чтобы при десериализации
    // сохранялось исходное время ответа, а не время чтения.
    [JsonConstructor]
    private Envelope(object? result, Errors? errorList, DateTime timeGenerated)
    {
        Result = result;
        ErrorList = errorList;
        TimeGenerated = timeGenerated;
    }

    public static Envelope Ok(object? result) =>
        new(result, null, DateTime.UtcNow);

    public static Envelope Error(Errors errors) =>
        new(null, errors, DateTime.UtcNow);
}

public record Envelope<T>
{
    public T? Result { get; }

    public Errors? ErrorList { get; }

    public bool IsError => ErrorList is { Count: > 0 };

    public DateTime TimeGenerated { get; }

    [JsonConstructor]
    private Envelope(T? result, Errors? errorList, DateTime timeGenerated)
    {
        Result = result;
        ErrorList = errorList;
        TimeGenerated = timeGenerated;
    }

    public static Envelope<T> Ok(T? result) =>
        new(result, null, DateTime.UtcNow);

    public static Envelope<T> Error(Errors errors) =>
        new(default, errors, DateTime.UtcNow);
}