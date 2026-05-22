using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Departments.Queries;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Contracts.Departments.Responses;
using FluentAssertions;
using NSubstitute;

namespace DirectoryService.UnitTests.Departments;

public class GetTopDepartmentsByPositionHandlerTests
{
    [Fact]
    public async Task Handle_should_route_through_cache_with_correct_key_and_tag()
    {
        var connectionFactory = Substitute.For<IDbConnectionFactory>();
        var cache = Substitute.For<ICacheService>();
        var request = new GetTopDepartmentsByPositionsRequest(RowsCount: 7);
        var query = new GetTopDepartmentsByPositionQuery(request);
        string expectedKey = DepartmentsCache.TopByPositionKey(request);
        var expected = new GetTopDepartmentsByPositionResponse(new List<DepartmentWithPositionCountDto>());

        cache.GetOrCreateAsync(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<GetTopDepartmentsByPositionResponse>>>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<IReadOnlyList<string>?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ValueTask<GetTopDepartmentsByPositionResponse>(expected));

        var sut = new GetTopDepartmentsByPositionHandler(connectionFactory, cache);

        var result = await sut.Handle(query, CancellationToken.None);

        result.Should().BeSameAs(expected);

        await cache.Received(1).GetOrCreateAsync(
            expectedKey,
            Arg.Any<Func<CancellationToken, ValueTask<GetTopDepartmentsByPositionResponse>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Is<IReadOnlyList<string>?>(tags => tags != null && tags.Contains(DepartmentsCache.Tag)),
            Arg.Any<CancellationToken>());

        connectionFactory.DidNotReceiveWithAnyArgs();
    }
}