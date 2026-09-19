using System.Text.Json;

namespace DirectoryService.IntegrationTests.Infrastructure;

/// <summary>
/// Чтение ответов API. Проверяем именно JSON, а не десериализованный Envelope:
/// так тесты фиксируют HTTP-контракт, который видят клиенты.
/// </summary>
public static class EnvelopeJson
{
    public static async Task<JsonElement> ReadAsync(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument document = JsonDocument.Parse(body);
        return document.RootElement.Clone();
    }

    public static Guid ResultAsGuid(this JsonElement envelope) =>
        envelope.GetProperty("result").GetGuid();

    public static IReadOnlyList<string> ErrorCodes(this JsonElement envelope) =>
        envelope.GetProperty("errorList")
            .EnumerateArray()
            .Select(error => error.GetProperty("code").GetString()!)
            .ToList();
}