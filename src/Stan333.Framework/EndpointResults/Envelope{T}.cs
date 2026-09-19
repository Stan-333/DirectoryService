using System.Text.Json.Serialization;
using Stan333.SharedKernel;

namespace Stan333.Framework.EndpointResults;

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