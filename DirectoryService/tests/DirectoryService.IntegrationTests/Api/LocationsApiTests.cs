using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;

namespace DirectoryService.IntegrationTests.Api;

// Тесты через HTTP: проверяют маршруты, привязку параметров, статусы и JSON ответа,
// то есть всё, что не видно при вызове хендлеров напрямую.
public class LocationsApiTests : DirectoryBaseTests
{
    private readonly HttpClient _client;

    public LocationsApiTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_location_with_valid_data_should_return_ok_and_save_location()
    {
        // Arrange
        CreateLocationRequest request = BuildRequest("Офис на Тверской", "1");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/locations", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement envelope = await EnvelopeJson.ReadAsync(response);
        Assert.False(envelope.GetProperty("isError").GetBoolean());

        Guid locationId = envelope.ResultAsGuid();
        Location? location = await ExecuteInDb(dbContext =>
            dbContext.Locations.FindAsync([new LocationId(locationId)]).AsTask());
        Assert.NotNull(location);
    }

    [Fact]
    public async Task Post_location_with_existing_name_should_return_conflict()
    {
        // Arrange
        HttpResponseMessage first = await _client.PostAsJsonAsync("/api/locations", BuildRequest("Склад", "2"));

        // Act
        HttpResponseMessage second = await _client.PostAsJsonAsync("/api/locations", BuildRequest("Склад", "3"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        JsonElement envelope = await EnvelopeJson.ReadAsync(second);
        Assert.True(envelope.GetProperty("isError").GetBoolean());
        Assert.Contains("record.already.exist", envelope.ErrorCodes());
    }

    [Fact]
    public async Task Post_location_with_invalid_data_should_return_bad_request()
    {
        // Arrange
        var request = new CreateLocationRequest(string.Empty, BuildAddress("4"), "Nowhere/Unknown");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/locations", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        JsonElement envelope = await EnvelopeJson.ReadAsync(response);
        Assert.Contains("value.is.required", envelope.ErrorCodes());
    }

    [Fact]
    public async Task Get_locations_should_bind_nested_pagination_from_query()
    {
        // Arrange
        await CreateManyLocationsAsync(3);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/locations?Pagination.Page=2&Pagination.PageSize=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement result = (await EnvelopeJson.ReadAsync(response)).GetProperty("result");
        Assert.Equal(1, result.GetProperty("locations").GetArrayLength());
        Assert.Equal(3, result.GetProperty("totalCount").GetInt64());
    }

    [Fact]
    public async Task Get_locations_should_clamp_out_of_range_pagination()
    {
        // Arrange
        await CreateManyLocationsAsync(3);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/locations?Pagination.Page=0&Pagination.PageSize=0");

        // Assert
        // Page=0 превращается в 1, PageSize=0 — в минимальный размер страницы 1.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement result = (await EnvelopeJson.ReadAsync(response)).GetProperty("result");
        Assert.Equal(1, result.GetProperty("locations").GetArrayLength());
        Assert.Equal(3, result.GetProperty("totalCount").GetInt64());
    }

    private static CreateLocationRequest BuildRequest(string name, string house) =>
        new(name, BuildAddress(house), "Europe/Moscow");

    private static AddressDto BuildAddress(string house) =>
        new()
        {
            PostalCode = "123456",
            Region = "г. Москва",
            City = "г. Москва",
            Street = "ул. Тверская",
            House = house,
        };
}