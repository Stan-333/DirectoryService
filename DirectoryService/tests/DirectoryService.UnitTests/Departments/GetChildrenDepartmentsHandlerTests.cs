using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Departments.Queries;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Contracts.Departments.Responses;
using FluentAssertions;
using NSubstitute;

namespace DirectoryService.UnitTests.Departments;

public class GetChildrenDepartmentsHandlerTests
{
    [Fact]
    public async Task Handle_should_route_through_cache_with_correct_key_and_tag()
    {
        var connectionFactory = Substitute.For<IDbConnectionFactory>();
        var cache = Substitute.For<ICacheService>();
        var parentId = Guid.NewGuid();
        var request = new GetChildrenDepartmentsRequest(Page: 2, PageSize: 15);
        var query = new GetChildrenDepartmentsQuery(parentId, request);
        string expectedKey = DepartmentsCache.ChildrenKey(parentId, request);
        var expected = new GetChildrenDepartmentsResponse(new List<DepartmentWithChildrenInfoDto>());

        cache.GetOrCreateAsync(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<GetChildrenDepartmentsResponse>>>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<IReadOnlyList<string>?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ValueTask<GetChildrenDepartmentsResponse>(expected));

        var sut = new GetChildrenDepartmentsHandler(connectionFactory, cache);

        var result = await sut.Handle(query, CancellationToken.None);

        result.Should().BeSameAs(expected);

        await cache.Received(1).GetOrCreateAsync(
            expectedKey,
            Arg.Any<Func<CancellationToken, ValueTask<GetChildrenDepartmentsResponse>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Is<IReadOnlyList<string>?>(tags => tags != null && tags.Contains(DepartmentsCache.Tag)),
            Arg.Any<CancellationToken>());

        // Если кэш отдал значение — фабрика не должна была лезть в БД
        connectionFactory.DidNotReceiveWithAnyArgs();
    }
}