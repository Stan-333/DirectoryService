using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Departments.Queries;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Contracts.Departments.Responses;
using FluentAssertions;
using NSubstitute;

namespace DirectoryService.UnitTests.Departments;

public class GetRootDepartmentsWithChildrenHandlerTests
{
    [Fact]
    public async Task Handle_should_route_through_cache_with_correct_key_and_tag()
    {
        var connectionFactory = Substitute.For<IDbConnectionFactory>();
        var cache = Substitute.For<ICacheService>();
        var request = new GetRootDepartmentsWithChildrenRequest(Page: 1, PageSize: 10, Prefetch: 5);
        var query = new GetRootDepartmentsWithChildrenQuery(request);
        string expectedKey = DepartmentsCache.RootsWithChildrenKey(request);
        var expected = new GetRootDepartmentsWithChildrenResponse(new List<DepartmentWithChildrenInfoDto>());

        cache.GetOrCreateAsync(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<GetRootDepartmentsWithChildrenResponse>>>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<IReadOnlyList<string>?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ValueTask<GetRootDepartmentsWithChildrenResponse>(expected));

        var sut = new GetRootDepartmentsWithChildrenHandler(connectionFactory, cache);

        var result = await sut.Handle(query, CancellationToken.None);

        result.Should().BeSameAs(expected);

        await cache.Received(1).GetOrCreateAsync(
            expectedKey,
            Arg.Any<Func<CancellationToken, ValueTask<GetRootDepartmentsWithChildrenResponse>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Is<IReadOnlyList<string>?>(tags => tags != null && tags.Contains(DepartmentsCache.Tag)),
            Arg.Any<CancellationToken>());

        connectionFactory.DidNotReceiveWithAnyArgs();
    }
}