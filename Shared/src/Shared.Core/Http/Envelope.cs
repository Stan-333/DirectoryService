using System.Text.Json.Serialization;
using Shared.Kernel;

namespace Shared.Core.Http;

public record Envelope
{
    // Ответы с ошибкой не содержат "result": null, иначе клиент не сможет прочитать их
    // как Envelope<T> с T-значимым типом (например, Envelope<Guid>).
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
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