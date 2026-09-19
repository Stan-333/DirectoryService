using System.Text.Json;
using System.Text.Json.Serialization;

namespace Stan333.SharedKernel;

/// <summary>
/// Читает и пишет <see cref="Errors"/> как JSON-массив ошибок.
/// Без него System.Text.Json не может создать <see cref="Errors"/> при десериализации.
/// </summary>
internal sealed class ErrorsJsonConverter : JsonConverter<Errors>
{
    public override Errors Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        List<Error> errors = JsonSerializer.Deserialize<List<Error>>(ref reader, options)
            ?? throw new JsonException("Ожидался JSON-массив ошибок.");

        return new Errors(errors);
    }

    public override void Write(Utf8JsonWriter writer, Errors value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (Error error in value)
        {
            JsonSerializer.Serialize(writer, error, options);
        }

        writer.WriteEndArray();
    }
}