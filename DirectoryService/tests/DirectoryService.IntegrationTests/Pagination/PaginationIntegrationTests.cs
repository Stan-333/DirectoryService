using DirectoryService.Application.Departments.Queries;
using DirectoryService.Application.Locations.Queries;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shared.Kernel;

namespace DirectoryService.IntegrationTests.Pagination;

public class PaginationIntegrationTests : DirectoryBaseTests
{
    public PaginationIntegrationTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Queries_with_maximum_pagination_values_should_return_empty_pages()
    {
        // Arrange
        var locationsQuery = new GetLocationsWithFiltersQuery(
            new GetLocationsWithFiltersRequest(
                null,
                null,
                null,
                new PaginationRequest(int.MaxValue, int.MaxValue)));
        var childrenQuery = new GetChildrenDepartmentsQuery(
            Guid.NewGuid(),
            new GetChildrenDepartmentsRequest(int.MaxValue, int.MaxValue));
        var rootsQuery = new GetRootDepartmentsWithChildrenQuery(
            new GetRootDepartmentsWithChildrenRequest(
                int.MaxValue,
                int.MaxValue,
                int.MaxValue));

        // Act
        var locations = await ExecuteHandler<GetLocationsWithFiltersHandler, PagedResult<GetLocationDto>>(
            (handler, cancellationToken) => handler.Handle(locationsQuery, cancellationToken));
        var children = await ExecuteHandler<GetChildrenDepartmentsHandler, GetChildrenDepartmentsResponse>(
            (handler, cancellationToken) => handler.Handle(childrenQuery, cancellationToken));
        var roots = await ExecuteHandler<GetRootDepartmentsWithChildrenHandler, GetRootDepartmentsWithChildrenResponse>(
            (handler, cancellationToken) => handler.Handle(rootsQuery, cancellationToken));

        // Assert
        Assert.Empty(locations.Items);
        Assert.Equal(PaginationConstraints.MaxPageSize, locations.PageSize);
        Assert.Empty(children.Departments);
        Assert.Empty(roots.Departments);
    }

    private async Task<TResult> ExecuteHandler<THandler, TResult>(
        Func<THandler, CancellationToken, Task<TResult>> action)
        where THandler : notnull
    {
        await using var scope = Services.CreateAsyncScope();
        THandler handler = scope.ServiceProvider.GetRequiredService<THandler>();

        return await action(handler, CancellationToken.None);
    }
}