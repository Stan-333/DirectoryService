using DirectoryService.Contracts;
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
    [InlineData(int.MaxValue, 107_374_183)]
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
    [InlineData(int.MaxValue, 107_374_183)]
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
    [InlineData(100, 100)]
    [InlineData(101, 100)]
    [InlineData(int.MaxValue, 100)]
    public void GetRootDepartmentsWithChildrenRequest_clamps_prefetch_to_valid_range(int input, int expected)
    {
        new GetRootDepartmentsWithChildrenRequest(Prefetch: input).Prefetch.Should().Be(expected);
    }

    [Fact]
    public void GetRootDepartmentsWithChildrenRequest_clamps_page_to_safe_offset()
    {
        var request = new GetRootDepartmentsWithChildrenRequest(Page: int.MaxValue, PageSize: 100);

        request.Page.Should().Be(21_474_837);
        PaginationConstraints.CalculateOffset(request.Page, request.PageSize)
            .Should().Be(2_147_483_600);
    }

    [Theory]
    [InlineData(1, 20, 0)]
    [InlineData(2, 20, 20)]
    [InlineData(0, 0, 0)]
    [InlineData(int.MaxValue, int.MaxValue, 2_147_483_600)]
    public void CalculateOffset_returns_safe_int(int page, int pageSize, int expected)
    {
        PaginationConstraints.CalculateOffset(page, pageSize).Should().Be(expected);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(0, 1)]
    [InlineData(5, 5)]
    [InlineData(100, 100)]
    [InlineData(101, 100)]
    [InlineData(int.MaxValue, 100)]
    public void GetTopDepartmentsByPositionsRequest_clamps_rows_count(int input, int expected)
    {
        new GetTopDepartmentsByPositionsRequest(RowsCount: input).RowsCount.Should().Be(expected);
    }
}