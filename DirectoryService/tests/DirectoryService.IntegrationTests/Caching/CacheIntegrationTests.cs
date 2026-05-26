using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.Queries;
using DirectoryService.Application.Departments.SoftDeleteDepartment;
using DirectoryService.Application.Departments.UpdateDepartmentLocations;
using DirectoryService.Application.Positions.CreatePosition;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace DirectoryService.IntegrationTests.Caching;

public class CacheIntegrationTests : DirectoryBaseTests
{
    public CacheIntegrationTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetChildren_should_return_cached_value_when_db_changed_outside_handler()
    {
        // Arrange — создаём дерево минуя CreateDepartmentHandler, чтобы он не сделал
        // RemoveByTagAsync уже после того, как мы наполнили кэш.
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationAsync(cancellationToken);
        (DepartmentId rootId, DepartmentId childId) = await CreateRootWithChildInDbAsync(
            "rootone",
            "childone",
            locationId,
            cancellationToken);

        var request = new GetChildrenDepartmentsRequest(Page: 1, PageSize: 20);

        // Act — первый GET наполняет кэш; затем удаляем ребёнка в обход хендлера.
        var first = await RunQuery((GetChildrenDepartmentsHandler h) =>
            h.Handle(new GetChildrenDepartmentsQuery(rootId.Value, request), cancellationToken));

        await HardDeleteDepartmentAsync(childId, cancellationToken);

        var second = await RunQuery((GetChildrenDepartmentsHandler h) =>
            h.Handle(new GetChildrenDepartmentsQuery(rootId.Value, request), cancellationToken));

        // Assert — кэш отдал тот же ответ, несмотря на изменения в БД.
        first.Departments.Should().HaveCount(1);
        first.Departments[0].DepartmentId.Should().Be(childId.Value);
        second.Departments.Should().HaveCount(1);
        second.Departments[0].DepartmentId.Should().Be(childId.Value);
    }

    [Fact]
    public async Task GetChildren_cache_keys_should_differentiate_pages()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationAsync(cancellationToken);
        (DepartmentId rootId, DepartmentId childAId, DepartmentId childBId) = await CreateRootWithTwoChildrenInDbAsync(
            "rooten",
            "childa",
            "childb",
            locationId,
            cancellationToken);

        var page1 = new GetChildrenDepartmentsRequest(Page: 1, PageSize: 1);
        var page2 = new GetChildrenDepartmentsRequest(Page: 2, PageSize: 1);

        // Act
        // 1. page=1 — MISS, кэшируется под ключом page=1
        var firstPage = await RunQuery((GetChildrenDepartmentsHandler h) =>
            h.Handle(new GetChildrenDepartmentsQuery(rootId.Value, page1), cancellationToken));

        // 2. Удаляем childA напрямую — кэш ничего не знает.
        await HardDeleteDepartmentAsync(childAId, cancellationToken);

        // 3. page=1 — HIT (тот же ключ), должны увидеть закэшированного childA.
        var firstPageAgain = await RunQuery((GetChildrenDepartmentsHandler h) =>
            h.Handle(new GetChildrenDepartmentsQuery(rootId.Value, page1), cancellationToken));

        // 4. page=2 — MISS (другой ключ): идёт в БД, видит, что childA удалён,
        //    childB сместился на page=1 — на page=2 уже ничего нет.
        var secondPage = await RunQuery((GetChildrenDepartmentsHandler h) =>
            h.Handle(new GetChildrenDepartmentsQuery(rootId.Value, page2), cancellationToken));

        // Assert
        firstPage.Departments.Should().ContainSingle().Which.DepartmentId.Should().Be(childAId.Value);
        firstPageAgain.Departments.Should().ContainSingle().Which.DepartmentId.Should().Be(childAId.Value);
        secondPage.Departments.Should().BeEmpty();

        // Sanity: childB всё ещё в БД, просто не попал в текущую страницу выборок.
        childBId.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateDepartment_should_invalidate_departments_cache()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationAsync(cancellationToken);
        var rootId = await CreateRootInDbAsync("rootcreate", locationId, cancellationToken);

        var request = new GetChildrenDepartmentsRequest();

        // Act — наполняем кэш пустым ответом.
        var beforeCreate = await RunQuery((GetChildrenDepartmentsHandler h) =>
            h.Handle(new GetChildrenDepartmentsQuery(rootId.Value, request), cancellationToken));

        // Создаём ребёнка через настоящий хендлер — внутри он должен вызвать RemoveByTagAsync.
        var newChildLocationId = await CreateLocationAsync(cancellationToken);
        Result<Guid, Errors> createResult = await RunQuery((CreateDepartmentHandler h) =>
            h.Handle(
                new CreateDepartmentCommand(new CreateDepartmentRequest(
                    "Новый ребёнок",
                    "newchild",
                    rootId.Value,
                    [newChildLocationId.Value])),
                cancellationToken));

        var afterCreate = await RunQuery((GetChildrenDepartmentsHandler h) =>
            h.Handle(new GetChildrenDepartmentsQuery(rootId.Value, request), cancellationToken));

        // Assert
        createResult.IsSuccess.Should().BeTrue();
        beforeCreate.Departments.Should().BeEmpty();
        afterCreate.Departments.Should().ContainSingle()
            .Which.DepartmentId.Should().Be(createResult.Value);
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_invalidate_departments_cache()
    {
        // Arrange — создаём root + active ребёнка.
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationAsync(cancellationToken);
        (DepartmentId rootId, DepartmentId childId) = await CreateRootWithChildInDbAsync(
            "rootdelete",
            "childdelete",
            locationId,
            cancellationToken);

        var rootsRequest = new GetRootDepartmentsWithChildrenRequest();

        // Act — кэшируем выдачу (root + 1 активный ребёнок).
        var before = await RunQuery((GetRootDepartmentsWithChildrenHandler h) =>
            h.Handle(new GetRootDepartmentsWithChildrenQuery(rootsRequest), cancellationToken));

        Result<Guid, Errors> softDeleteResult = await RunQuery((SoftDeleteDepartmentHandler h) =>
            h.Handle(new SoftDeleteDepartmentCommand(childId.Value), cancellationToken));

        var after = await RunQuery((GetRootDepartmentsWithChildrenHandler h) =>
            h.Handle(new GetRootDepartmentsWithChildrenQuery(rootsRequest), cancellationToken));

        // Assert — после soft delete child выпадает из выдачи (is_active=true в SQL).
        softDeleteResult.IsSuccess.Should().BeTrue();
        before.Departments.Should().HaveCount(2);
        before.Departments.Select(d => d.DepartmentId).Should().Contain(childId.Value);
        after.Departments.Should().ContainSingle()
            .Which.DepartmentId.Should().Be(rootId.Value);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_should_invalidate_departments_cache()
    {
        // Arrange — root + 1 ребёнок.
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationAsync(cancellationToken);
        (DepartmentId rootId, DepartmentId childId) = await CreateRootWithChildInDbAsync(
            "rootloc",
            "childloc",
            locationId,
            cancellationToken);

        var request = new GetChildrenDepartmentsRequest();

        // Act — кэшируем выдачу.
        var before = await RunQuery((GetChildrenDepartmentsHandler h) =>
            h.Handle(new GetChildrenDepartmentsQuery(rootId.Value, request), cancellationToken));

        // Удаляем ребёнка в обход хендлера, чтобы наблюдать факт инвалидации.
        await HardDeleteDepartmentAsync(childId, cancellationToken);

        // UpdateLocations на root → инвалидация кэша департаментов.
        var newLocationId = await CreateLocationAsync(cancellationToken);
        Result<Guid, Errors> updateResult = await RunQuery((UpdateDepartmentLocationsHandler h) =>
            h.Handle(
                new UpdateDepartmentLocationsCommand(
                    rootId.Value,
                    new UpdateDepartmentLocationsRequest([newLocationId.Value])),
                cancellationToken));

        var after = await RunQuery((GetChildrenDepartmentsHandler h) =>
            h.Handle(new GetChildrenDepartmentsQuery(rootId.Value, request), cancellationToken));

        // Assert — без инвалидации после второго запроса всё ещё был бы childId из кэша.
        updateResult.IsSuccess.Should().BeTrue();
        before.Departments.Should().ContainSingle()
            .Which.DepartmentId.Should().Be(childId.Value);
        after.Departments.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRootDepartmentsWithChildren_should_use_cache_and_invalidate_on_create()
    {
        // Arrange — один root без детей.
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationAsync(cancellationToken);
        var rootId = await CreateRootInDbAsync("rootroot", locationId, cancellationToken);

        var request = new GetRootDepartmentsWithChildrenRequest();

        // Act
        var before = await RunQuery((GetRootDepartmentsWithChildrenHandler h) =>
            h.Handle(new GetRootDepartmentsWithChildrenQuery(request), cancellationToken));

        // Создаём ребёнка через хендлер → инвалидация.
        var childLocationId = await CreateLocationAsync(cancellationToken);
        await RunQuery((CreateDepartmentHandler h) =>
            h.Handle(
                new CreateDepartmentCommand(new CreateDepartmentRequest(
                    "Ребёнок",
                    "rootchild",
                    rootId.Value,
                    [childLocationId.Value])),
                cancellationToken));

        var after = await RunQuery((GetRootDepartmentsWithChildrenHandler h) =>
            h.Handle(new GetRootDepartmentsWithChildrenQuery(request), cancellationToken));

        // Assert
        before.Departments.Should().ContainSingle()
            .Which.DepartmentId.Should().Be(rootId.Value);
        after.Departments.Should().HaveCount(2);
        after.Departments.Select(d => d.DepartmentId).Should().Contain(rootId.Value);
    }

    [Fact]
    public async Task CreatePosition_should_invalidate_top_by_position_cache()
    {
        // top-by-position считает позиции из department_positions:
        // если новая позиция привязана к department-у, его счётчик меняется.
        // CreatePositionHandler обязан инвалидировать общий тег "departments".
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationAsync(cancellationToken);
        var rootId = await CreateRootInDbAsync("rootpos", locationId, cancellationToken);

        var request = new GetTopDepartmentsByPositionsRequest(RowsCount: 5);

        // Act — наполняем кэш пустой выдачей (нет позиций — нет строк).
        var before = await RunQuery((GetTopDepartmentsByPositionHandler h) =>
            h.Handle(new GetTopDepartmentsByPositionQuery(request), cancellationToken));

        // Создаём позицию через хендлер → инвалидация тега departments.
        Result<Guid, Errors> createResult = await RunQuery((CreatePositionHandler h) =>
            h.Handle(
                new CreatePositionCommand(new CreatePositionRequest(
                    "Менеджер",
                    "Описание",
                    [rootId.Value])),
                cancellationToken));

        var after = await RunQuery((GetTopDepartmentsByPositionHandler h) =>
            h.Handle(new GetTopDepartmentsByPositionQuery(request), cancellationToken));

        // Assert — без инвалидации `after` остался бы пустым из кэша.
        createResult.IsSuccess.Should().BeTrue();
        before.Departments.Should().BeEmpty();
        after.Departments.Should().ContainSingle()
            .Which.DepartmentId.Should().Be(rootId.Value);
    }

    [Fact]
    public void DepartmentsCache_keys_should_include_filter_parameters()
    {
        // Контракт: ключи зависят от параметров запроса (страница, размер, prefetch, parentId).
        // Это страховка от случайного дрифта формул генерации ключей.
        var parentId = Guid.NewGuid();
        string childA = DepartmentsCache.ChildrenKey(parentId, new GetChildrenDepartmentsRequest(1, 20));
        string childB = DepartmentsCache.ChildrenKey(parentId, new GetChildrenDepartmentsRequest(2, 20));
        string childC = DepartmentsCache.ChildrenKey(parentId, new GetChildrenDepartmentsRequest(1, 50));

        string rootsA = DepartmentsCache.RootsWithChildrenKey(new GetRootDepartmentsWithChildrenRequest(1, 20, 3));
        string rootsB = DepartmentsCache.RootsWithChildrenKey(new GetRootDepartmentsWithChildrenRequest(1, 20, 5));

        childA.Should().NotBe(childB);
        childA.Should().NotBe(childC);
        rootsA.Should().NotBe(rootsB);
    }

    private async Task<TResult> RunQuery<THandler, TResult>(Func<THandler, Task<TResult>> action)
        where THandler : notnull
    {
        await using var scope = Services.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<THandler>();
        return await action(handler);
    }

    private async Task<DepartmentId> CreateRootInDbAsync(
        string identifierValue,
        LocationId locationId,
        CancellationToken cancellationToken)
    {
        var rootId = new DepartmentId(Guid.NewGuid());

        await ExecuteInDb(async dbContext =>
        {
            var root = Department.CreateParent(
                DepartmentName.Create($"Корень-{identifierValue}").Value,
                Identifier.Create(identifierValue).Value,
                [new DepartmentLocation(rootId, locationId)],
                rootId);

            await dbContext.Departments.AddAsync(root.Value, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        });

        return rootId;
    }

    private async Task<(DepartmentId Root, DepartmentId Child)> CreateRootWithChildInDbAsync(
        string rootIdentifier,
        string childIdentifier,
        LocationId locationId,
        CancellationToken cancellationToken)
    {
        var rootId = new DepartmentId(Guid.NewGuid());
        var childId = new DepartmentId(Guid.NewGuid());

        await ExecuteInDb(async dbContext =>
        {
            var root = Department.CreateParent(
                DepartmentName.Create($"Корень-{rootIdentifier}").Value,
                Identifier.Create(rootIdentifier).Value,
                [new DepartmentLocation(rootId, locationId)],
                rootId);

            var child = Department.CreateChild(
                DepartmentName.Create($"Ребёнок-{childIdentifier}").Value,
                Identifier.Create(childIdentifier).Value,
                root.Value,
                [new DepartmentLocation(childId, locationId)],
                childId);

            await dbContext.Departments.AddRangeAsync([root.Value, child.Value], cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        });

        return (rootId, childId);
    }

    private async Task<(DepartmentId Root, DepartmentId ChildA, DepartmentId ChildB)> CreateRootWithTwoChildrenInDbAsync(
        string rootIdentifier,
        string childAIdentifier,
        string childBIdentifier,
        LocationId locationId,
        CancellationToken cancellationToken)
    {
        var rootId = new DepartmentId(Guid.NewGuid());
        var childAId = new DepartmentId(Guid.NewGuid());
        var childBId = new DepartmentId(Guid.NewGuid());

        await ExecuteInDb(async dbContext =>
        {
            var root = Department.CreateParent(
                DepartmentName.Create($"Корень-{rootIdentifier}").Value,
                Identifier.Create(rootIdentifier).Value,
                [new DepartmentLocation(rootId, locationId)],
                rootId);

            var childA = Department.CreateChild(
                DepartmentName.Create($"Ребёнок-{childAIdentifier}").Value,
                Identifier.Create(childAIdentifier).Value,
                root.Value,
                [new DepartmentLocation(childAId, locationId)],
                childAId);

            // Гарантируем, что childA «старше» childB по created_at,
            // чтобы он шёл первым в запросе с ORDER BY created_at.
            await dbContext.Departments.AddRangeAsync([root.Value, childA.Value], cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            var childB = Department.CreateChild(
                DepartmentName.Create($"Ребёнок-{childBIdentifier}").Value,
                Identifier.Create(childBIdentifier).Value,
                root.Value,
                [new DepartmentLocation(childBId, locationId)],
                childBId);

            await dbContext.Departments.AddAsync(childB.Value, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        });

        return (rootId, childAId, childBId);
    }

    private async Task HardDeleteDepartmentAsync(DepartmentId id, CancellationToken cancellationToken)
    {
        await ExecuteInDb(async dbContext =>
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM department_locations WHERE department_id = {id.Value};",
                cancellationToken);

            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM departments WHERE department_id = {id.Value};",
                cancellationToken);
        });
    }
}