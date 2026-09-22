using System.Text.Json;
using FluentAssertions;
using Shared.Core.Http;
using Shared.Kernel;

namespace Shared.UnitTests.Core.Http;

public class EnvelopeTests
{
    private static JsonSerializerOptions WebOptions => new(JsonSerializerDefaults.Web);

    [Fact]
    public void Envelope_keeps_original_time_after_deserialization()
    {
        Envelope<string> original = Envelope<string>.Ok("value");

        string json = JsonSerializer.Serialize(original, WebOptions);
        Envelope<string> restored = JsonSerializer.Deserialize<Envelope<string>>(json, WebOptions)!;

        restored.TimeGenerated.Should().Be(original.TimeGenerated);
        restored.Result.Should().Be("value");
    }

    [Fact]
    public void Error_envelope_without_result_is_readable_as_envelope_of_value_type()
    {
        string json = JsonSerializer.Serialize(Envelope.Error(Error.NotFound(null, "Нет")), WebOptions);

        Envelope<Guid> envelope = JsonSerializer.Deserialize<Envelope<Guid>>(json, WebOptions)!;

        envelope.IsError.Should().BeTrue();
        envelope.ErrorList.Should().ContainSingle().Which.Type.Should().Be(ErrorType.NotFound);
    }
}