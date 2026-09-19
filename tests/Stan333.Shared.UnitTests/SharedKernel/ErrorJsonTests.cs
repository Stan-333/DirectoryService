using System.Text.Json;
using FluentAssertions;
using Stan333.SharedKernel;

namespace Stan333.Shared.UnitTests.SharedKernel;

public class ErrorJsonTests
{
    private static JsonSerializerOptions WebOptions => new(JsonSerializerDefaults.Web);

    [Fact]
    public void Error_type_is_serialized_as_string()
    {
        string json = JsonSerializer.Serialize(Error.NotFound(null, "Не найдено"), WebOptions);

        json.Should().Contain("\"type\":\"NotFound\"");
    }

    [Fact]
    public void Error_round_trips_through_json()
    {
        Error error = Error.Validation("value.is.invalid", "Некорректное значение", "Name");

        string json = JsonSerializer.Serialize(error, WebOptions);
        Error? restored = JsonSerializer.Deserialize<Error>(json, WebOptions);

        restored.Should().Be(error);
    }

    [Fact]
    public void Errors_round_trip_as_json_array()
    {
        Errors errors = new[] { Error.NotFound(null, "a"), Error.Conflict(null, "b") };

        string json = JsonSerializer.Serialize(errors, WebOptions);
        Errors? restored = JsonSerializer.Deserialize<Errors>(json, WebOptions);

        json.Should().StartWith("[");
        restored.Should().NotBeNull();
        restored!.Should().Equal(errors);
    }
}