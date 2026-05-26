using DirectoryService.Application.Departments;
using DirectoryService.Contracts.Departments.Requests;
using FluentAssertions;

namespace DirectoryService.UnitTests.Caching;

public class DepartmentsCacheTests
{
    [Fact]
    public void Tag_should_be_stable_value()
    {
        DepartmentsCache.Tag.Should().Be("departments");
        DepartmentsCache.Tags.Should().ContainSingle().Which.Should().Be("departments");
    }

    [Fact]
    public void ChildrenKey_should_include_parentId_and_paging()
    {
        var parent = Guid.Parse("11111111-1111-1111-1111-111111111111");

        string key = DepartmentsCache.ChildrenKey(parent, new GetChildrenDepartmentsRequest(Page: 3, PageSize: 25));

        key.Should().Be("departments:children:11111111111111111111111111111111:p=3:ps=25");
    }

    [Fact]
    public void ChildrenKey_should_differ_for_different_pages()
    {
        var parent = Guid.NewGuid();

        string keyA = DepartmentsCache.ChildrenKey(parent, new GetChildrenDepartmentsRequest(Page: 1, PageSize: 20));
        string keyB = DepartmentsCache.ChildrenKey(parent, new GetChildrenDepartmentsRequest(Page: 2, PageSize: 20));

        keyA.Should().NotBe(keyB);
    }

    [Fact]
    public void ChildrenKey_should_differ_for_different_parents()
    {
        var request = new GetChildrenDepartmentsRequest();

        string keyA = DepartmentsCache.ChildrenKey(Guid.NewGuid(), request);
        string keyB = DepartmentsCache.ChildrenKey(Guid.NewGuid(), request);

        keyA.Should().NotBe(keyB);
    }

    [Fact]
    public void RootsWithChildrenKey_should_include_paging_and_prefetch()
    {
        string key = DepartmentsCache.RootsWithChildrenKey(
            new GetRootDepartmentsWithChildrenRequest(Page: 2, PageSize: 10, Prefetch: 7));

        key.Should().Be("departments:roots:p=2:ps=10:pf=7");
    }

    [Fact]
    public void RootsWithChildrenKey_should_differ_for_different_prefetch()
    {
        string keyA = DepartmentsCache.RootsWithChildrenKey(new GetRootDepartmentsWithChildrenRequest(Prefetch: 3));
        string keyB = DepartmentsCache.RootsWithChildrenKey(new GetRootDepartmentsWithChildrenRequest(Prefetch: 5));

        keyA.Should().NotBe(keyB);
    }

    [Fact]
    public void TopByPositionKey_should_use_rows_count()
    {
        string key = DepartmentsCache.TopByPositionKey(new GetTopDepartmentsByPositionsRequest(RowsCount: 10));

        key.Should().Be("departments:top-by-position:rc=10");
    }

    [Fact]
    public void Keys_for_different_endpoints_should_share_common_prefix_but_differ()
    {
        string children = DepartmentsCache.ChildrenKey(Guid.NewGuid(), new GetChildrenDepartmentsRequest());
        string roots = DepartmentsCache.RootsWithChildrenKey(new GetRootDepartmentsWithChildrenRequest());
        string top = DepartmentsCache.TopByPositionKey(new GetTopDepartmentsByPositionsRequest());

        children.Should().StartWith("departments:");
        roots.Should().StartWith("departments:");
        top.Should().StartWith("departments:");
        new[] { children, roots, top }.Distinct().Should().HaveCount(3);
    }
}