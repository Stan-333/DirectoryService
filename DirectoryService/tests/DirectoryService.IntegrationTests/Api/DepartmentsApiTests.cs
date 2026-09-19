using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;

namespace DirectoryService.IntegrationTests.Api;

public class DepartmentsApiTests : DirectoryBaseTests
{
    private readonly HttpClient _client;

    public DepartmentsApiTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_root_department_should_return_ok_and_appear_in_roots()
    {
        // Arrange
        LocationId locationId = await CreateLocationAsync();

        // Act
        Guid departmentId = await CreateDepartmentAsync("Головной офис", "head_office", null, locationId);
        HttpResponseMessage roots = await _client.GetAsync("/api/departments/roots?Page=1&PageSize=10&Prefetch=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, roots.StatusCode);
        Assert.Contains(departmentId, DepartmentIds(await EnvelopeJson.ReadAsync(roots)));
    }

    [Fact]
    public async Task Post_department_with_unknown_location_should_return_not_found()
    {
        // Arrange
        var request = new CreateDepartmentRequest("Отдел", "unknown_location", null, [Guid.NewGuid()]);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/departments", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("location.not.found", (await EnvelopeJson.ReadAsync(response)).ErrorCodes());
    }

    [Fact]
    public async Task Get_children_should_return_child_department()
    {
        // Arrange
        LocationId locationId = await CreateLocationAsync();
        Guid parentId = await CreateDepartmentAsync("Дирекция", "directorate", null, locationId);
        Guid childId = await CreateDepartmentAsync("Бухгалтерия", "accounting", parentId, locationId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/departments/{parentId}/children?Page=1&PageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(childId, DepartmentIds(await EnvelopeJson.ReadAsync(response)));
    }

    [Fact]
    public async Task Delete_child_department_should_remove_it_from_children()
    {
        // Arrange
        LocationId locationId = await CreateLocationAsync();
        Guid parentId = await CreateDepartmentAsync("Дирекция", "directorate", null, locationId);
        Guid childId = await CreateDepartmentAsync("Бухгалтерия", "accounting", parentId, locationId);

        // Act
        HttpResponseMessage delete = await _client.DeleteAsync($"/api/departments/{childId}");
        HttpResponseMessage children = await _client.GetAsync($"/api/departments/{parentId}/children?Page=1&PageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);
        Assert.Equal(HttpStatusCode.OK, children.StatusCode);
        Assert.DoesNotContain(childId, DepartmentIds(await EnvelopeJson.ReadAsync(children)));
    }

    [Fact]
    public async Task Put_parent_to_own_child_should_return_conflict()
    {
        // Arrange
        LocationId locationId = await CreateLocationAsync();
        Guid parentId = await CreateDepartmentAsync("Дирекция", "directorate", null, locationId);
        Guid childId = await CreateDepartmentAsync("Бухгалтерия", "accounting", parentId, locationId);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"/api/departments/{parentId}/parent",
            new UpdateDepartmentParentRequest(childId));

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("parent.is.conflict", (await EnvelopeJson.ReadAsync(response)).ErrorCodes());
    }

    [Fact]
    public async Task Delete_department_should_return_ok_and_remove_it_from_cached_roots()
    {
        // Arrange
        LocationId locationId = await CreateLocationAsync();
        Guid departmentId = await CreateDepartmentAsync("Архив", "archive", null, locationId);

        // Первый запрос кладёт список корневых подразделений в кэш
        HttpResponseMessage rootsBefore = await _client.GetAsync("/api/departments/roots?Page=1&PageSize=10");

        // Act
        HttpResponseMessage delete = await _client.DeleteAsync($"/api/departments/{departmentId}");
        HttpResponseMessage rootsAfter = await _client.GetAsync("/api/departments/roots?Page=1&PageSize=10");

        // Assert
        Assert.Contains(departmentId, DepartmentIds(await EnvelopeJson.ReadAsync(rootsBefore)));
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);
        Assert.DoesNotContain(departmentId, DepartmentIds(await EnvelopeJson.ReadAsync(rootsAfter)));
    }

    [Fact]
    public async Task Get_top_positions_should_count_created_position()
    {
        // Arrange
        LocationId locationId = await CreateLocationAsync();
        Guid departmentId = await CreateDepartmentAsync("Отдел кадров", "human_resources", null, locationId);

        // Act
        HttpResponseMessage position = await _client.PostAsJsonAsync(
            "/api/positions",
            new CreatePositionRequest("Кадровик", null, [departmentId]));
        HttpResponseMessage top = await _client.GetAsync("/api/departments/top-positions?RowsCount=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, position.StatusCode);
        Assert.Equal(HttpStatusCode.OK, top.StatusCode);
        JsonElement department = (await EnvelopeJson.ReadAsync(top))
            .GetProperty("result")
            .GetProperty("departments")
            .EnumerateArray()
            .Single(d => d.GetProperty("departmentId").GetGuid() == departmentId);
        Assert.Equal(1, department.GetProperty("positionCount").GetInt32());
    }

    private static IReadOnlyList<Guid> DepartmentIds(JsonElement envelope) =>
        envelope.GetProperty("result")
            .GetProperty("departments")
            .EnumerateArray()
            .Select(department => department.GetProperty("departmentId").GetGuid())
            .ToList();

    private async Task<Guid> CreateDepartmentAsync(string name, string identifier, Guid? parentId, LocationId locationId)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/departments",
            new CreateDepartmentRequest(name, identifier, parentId, [locationId.Value]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await EnvelopeJson.ReadAsync(response)).ResultAsGuid();
    }
}