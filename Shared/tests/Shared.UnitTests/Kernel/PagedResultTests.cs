using System.Text.Json;
using FluentAssertions;
using Shared.Kernel;

namespace Shared.UnitTests.Kernel;

public class PagedResultTests
{
    private static JsonSerializerOptions WebOptions => new(JsonSerializerDefaults.Web);

    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(40, 20, 2)]
    [InlineData(45, 20, 3)]
    public void TotalPages_rounds_up(long totalCount, int pageSize, long expected)
    {
        var page = new PagedResult<int>([], totalCount, 1, pageSize);

        page.TotalPages.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, false, true)]
    [InlineData(2, true, true)]
    [InlineData(3, true, false)]
    public void Neighbour_pages_are_known(int pageNumber, bool hasPrevious, bool hasNext)
    {
        var page = new PagedResult<int>([1], 45, pageNumber, 20);

        page.HasPreviousPage.Should().Be(hasPrevious);
        page.HasNextPage.Should().Be(hasNext);
    }

    [Fact]
    public void Empty_list_has_no_neighbour_pages()
    {
        var page = new PagedResult<int>([], 0, 1, 20);

        page.HasPreviousPage.Should().BeFalse();
        page.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Invalid_arguments_are_rejected()
    {
        FluentActions.Invoking(() => new PagedResult<int>(null!, 0, 1, 20)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new PagedResult<int>([], -1, 1, 20)).Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => new PagedResult<int>([], 0, 0, 20)).Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => new PagedResult<int>([], 0, 1, 0)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Json_contains_paging_fields_and_round_trips()
    {
        var original = new PagedResult<string>(["a", "b"], 45, 2, 20);

        string json = JsonSerializer.Serialize(original, WebOptions);
        PagedResult<string> restored = JsonSerializer.Deserialize<PagedResult<string>>(json, WebOptions)!;

        using JsonDocument document = JsonDocument.Parse(json);
        document.RootElement.EnumerateObject().Select(p => p.Name).Should().BeEquivalentTo(
            ["items", "totalCount", "page", "pageSize", "totalPages", "hasPreviousPage", "hasNextPage"]);
        restored.Should().BeEquivalentTo(original);
    }
}