using DirectoryService.Domain.Locations;
using FluentAssertions;
using TimeZone = DirectoryService.Domain.Locations.TimeZone;

namespace DirectoryService.UnitTests.Locations;

public class LocationTests
{
    [Fact]
    public void Create_with_current_utc_time_should_succeed()
    {
        var (name, address, timezone) = ValidParts();

        var result = Location.Create(name, address, timezone, true, DateTime.UtcNow);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_with_past_created_at_should_succeed()
    {
        var (name, address, timezone) = ValidParts();

        var result = Location.Create(name, address, timezone, true, DateTime.UtcNow.AddHours(-1));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_with_future_created_at_should_fail()
    {
        var (name, address, timezone) = ValidParts();

        var result = Location.Create(name, address, timezone, true, DateTime.UtcNow.AddDays(1));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("value.is.invalid");
    }

    private static (LocationName Name, Address Address, TimeZone Timezone) ValidParts()
    {
        var name = LocationName.Create("Локация").Value;
        var address = Address.Create("000000", "г. Москва", "г. Москва", "ул. Ленина", "1", "1").Value;
        var timezone = TimeZone.Create("Europe/Moscow").Value;
        return (name, address, timezone);
    }
}