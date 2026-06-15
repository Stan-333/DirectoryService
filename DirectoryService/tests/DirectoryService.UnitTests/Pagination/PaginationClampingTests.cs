using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Contracts.Locations.Requests;
using FluentAssertions;

namespace DirectoryService.UnitTests.Pagination;

public class PaginationClampingTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(1, 1)]
    [InlineData(7, 7)]
    public void PaginationRequest_clamps_page_to_at_least_one(int input, int expected)
    {
        new PaginationRequest(Page: input).Page.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(20, 20)]
    [InlineData(100, 100)]
    [InlineData(101, 100)]
    [InlineData(1_000_000, 100)]
    public void PaginationRequest_clamps_page_size_to_valid_range(int input, int expected)
    {
        new PaginationRequest(PageSize: input).PageSize.Should().Be(expected);
    }

    [Fact]
    public void PaginationRequest_keeps_default_values()
    {
        var request = new PaginationRequest();

        request.Page.Should().Be(1);
        request.PageSize.Should().Be(20);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(101, 100)]
    public void GetChildrenDepartmentsRequest_clamps_page_size(int input, int expected)
    {
        new GetChildrenDepartmentsRequest(PageSize: input).PageSize.Should().Be(expected);
    }

    [Theory]
    [InlineData(-3, 1)]
    [InlineData(4, 4)]
    public void GetChildrenDepartmentsRequest_clamps_page(int input, int expected)
    {
        new GetChildrenDepartmentsRequest(Page: input).Page.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1_000, 100)]
    public void GetRootDepartmentsWithChildrenRequest_clamps_page_size(int input, int expected)
    {
        new GetRootDepartmentsWithChildrenRequest(PageSize: input).PageSize.Should().Be(expected);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(5, 5)]
    public void GetRootDepartmentsWithChildrenRequest_clamps_prefetch_to_non_negative(int input, int expected)
    {
        new GetRootDepartmentsWithChildrenRequest(Prefetch: input).Prefetch.Should().Be(expected);
    }
}