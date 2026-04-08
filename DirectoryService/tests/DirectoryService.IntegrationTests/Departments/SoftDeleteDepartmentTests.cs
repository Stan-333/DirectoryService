using DirectoryService.Application.Departments.SoftDeleteDepartment;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class SoftDeleteDepartmentTests : DirectoryBaseTests
{
    public SoftDeleteDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_valid_data_should_success()
    {
        var cancellationToken = CancellationToken.None;
        var department = await CreateParentDepartmentAsync("parent", cancellationToken);

        var result = await ExecuteHandler(sut =>
            sut.Handle(new SoftDeleteDepartmentCommand(department.Id.Value), cancellationToken));

        await ExecuteInDb(async dbContext =>
        {
            var actualDepartment = await dbContext.Departments
                .SingleAsync(d => d.Id == department.Id, cancellationToken);

            Assert.False(actualDepartment.IsActive);
            Assert.NotNull(actualDepartment.DeletedAt);
            Assert.StartsWith("deleted_", actualDepartment.Path);
            Assert.True(result.IsSuccess);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_update_descendants_paths()
    {
        var cancellationToken = CancellationToken.None;
        var departments = await CreateDepartmentTreeAsync(cancellationToken);
        var parent = departments[0];
        var child = departments[1];
        var grandChild = departments[2];

        var result = await ExecuteHandler(sut =>
            sut.Handle(new SoftDeleteDepartmentCommand(parent.Id.Value), cancellationToken));

        await ExecuteInDb(async dbContext =>
        {
            var actualParent = await dbContext.Departments
                .SingleAsync(d => d.Id == parent.Id, cancellationToken);
            var actualChild = await dbContext.Departments
                .SingleAsync(d => d.Id == child.Id, cancellationToken);
            var actualGrandChild = await dbContext.Departments
                .SingleAsync(d => d.Id == grandChild.Id, cancellationToken);

            Assert.Equal($"deleted_{parent.Identifier.Value}", actualParent.Path);
            Assert.Equal($"{actualParent.Path}.{child.Identifier.Value}", actualChild.Path);
            Assert.Equal($"{actualChild.Path}.{grandChild.Identifier.Value}", actualGrandChild.Path);
            Assert.True(actualChild.IsActive);
            Assert.True(actualGrandChild.IsActive);
            Assert.True(result.IsSuccess);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_non_existing_id_should_fail()
    {
        var cancellationToken = CancellationToken.None;

        var result = await ExecuteHandler(sut =>
            sut.Handle(new SoftDeleteDepartmentCommand(Guid.NewGuid()), cancellationToken));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task SoftDeleteDepartment_with_already_deleted_department_should_fail()
    {
        var cancellationToken = CancellationToken.None;
        var department = await CreateParentDepartmentAsync("parent", cancellationToken);

        var firstResult = await ExecuteHandler(sut =>
            sut.Handle(new SoftDeleteDepartmentCommand(department.Id.Value), cancellationToken));
        var secondResult = await ExecuteHandler(sut =>
            sut.Handle(new SoftDeleteDepartmentCommand(department.Id.Value), cancellationToken));

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsFailure);
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_soft_delete_locations_used_only_by_this_department()
    {
        var cancellationToken = CancellationToken.None;
        var department = await CreateParentDepartmentAsync("parent", cancellationToken);
        var locationId = department.LocationIds.Single();

        var result = await ExecuteHandler(sut =>
            sut.Handle(new SoftDeleteDepartmentCommand(department.Id.Value), cancellationToken));

        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .SingleAsync(l => l.Id == locationId, cancellationToken);

            Assert.False(location.IsActive);
            Assert.NotNull(location.DeletedAt);
            Assert.True(result.IsSuccess);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_not_soft_delete_locations_used_by_other_active_departments()
    {
        var cancellationToken = CancellationToken.None;
        var sharedLocationId = await CreateLocationAsync(cancellationToken);
        var firstDepartment = await CreateParentDepartmentAsync("first", cancellationToken, [sharedLocationId]);
        _ = await CreateParentDepartmentAsync("second", cancellationToken, [sharedLocationId]);

        var result = await ExecuteHandler(sut =>
            sut.Handle(new SoftDeleteDepartmentCommand(firstDepartment.Id.Value), cancellationToken));

        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .SingleAsync(l => l.Id == sharedLocationId, cancellationToken);

            Assert.True(location.IsActive);
            Assert.Null(location.DeletedAt);
            Assert.True(result.IsSuccess);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_soft_delete_positions_used_only_by_this_department()
    {
        var cancellationToken = CancellationToken.None;
        var department = await CreateParentDepartmentAsync("parent", cancellationToken);
        var positionId = await CreatePositionAsync("Manager", [department.Id], cancellationToken);

        var result = await ExecuteHandler(sut =>
            sut.Handle(new SoftDeleteDepartmentCommand(department.Id.Value), cancellationToken));

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .SingleAsync(p => p.Id == positionId, cancellationToken);

            Assert.False(position.IsActive);
            Assert.NotNull(position.DeletedAt);
            Assert.True(result.IsSuccess);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_not_soft_delete_positions_used_by_other_active_departments()
    {
        var cancellationToken = CancellationToken.None;
        var firstDepartment = await CreateParentDepartmentAsync("first", cancellationToken);
        var secondDepartment = await CreateParentDepartmentAsync("second", cancellationToken);
        var positionId = await CreatePositionAsync("Manager", [firstDepartment.Id, secondDepartment.Id], cancellationToken);

        var result = await ExecuteHandler(sut =>
            sut.Handle(new SoftDeleteDepartmentCommand(firstDepartment.Id.Value), cancellationToken));

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .SingleAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(position.IsActive);
            Assert.Null(position.DeletedAt);
            Assert.True(result.IsSuccess);
        });
    }

    private async Task<T> ExecuteHandler<T>(Func<SoftDeleteDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<SoftDeleteDepartmentHandler>();

        return await action(sut);
    }

    private async Task<(DepartmentId Id, List<LocationId> LocationIds)> CreateParentDepartmentAsync(
        string identifierValue,
        CancellationToken cancellationToken,
        List<LocationId>? locationIds = null)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var actualLocationIds = locationIds ?? [await CreateLocationAsync(cancellationToken)];
            var departmentId = new DepartmentId(Guid.NewGuid());
            var department = Department.CreateParent(
                DepartmentName.Create($"Department-{identifierValue}").Value,
                Identifier.Create(identifierValue).Value,
                actualLocationIds.Select(locationId => new DepartmentLocation(departmentId, locationId)).ToList(),
                departmentId);

            await dbContext.Departments.AddAsync(department.Value, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return (departmentId, actualLocationIds);
        });
    }

    private async Task<List<Department>> CreateDepartmentTreeAsync(CancellationToken cancellationToken)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var parentLocation = await CreateLocationAsync(cancellationToken);
            var childLocation = await CreateLocationAsync(cancellationToken);
            var grandChildLocation = await CreateLocationAsync(cancellationToken);

            var parentId = new DepartmentId(Guid.NewGuid());
            var parent = Department.CreateParent(
                DepartmentName.Create("Department-parent").Value,
                Identifier.Create("parent").Value,
                [new DepartmentLocation(parentId, parentLocation)],
                parentId).Value;

            var childId = new DepartmentId(Guid.NewGuid());
            var child = Department.CreateChild(
                DepartmentName.Create("Department-child").Value,
                Identifier.Create("child").Value,
                parent,
                [new DepartmentLocation(childId, childLocation)],
                childId).Value;

            var grandChildId = new DepartmentId(Guid.NewGuid());
            var grandChild = Department.CreateChild(
                DepartmentName.Create("Department-grandchild").Value,
                Identifier.Create("grandchild").Value,
                child,
                [new DepartmentLocation(grandChildId, grandChildLocation)],
                grandChildId).Value;

            await dbContext.Departments.AddRangeAsync([parent, child, grandChild], cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new List<Department> { parent, child, grandChild };
        });
    }

    private async Task<PositionId> CreatePositionAsync(
        string name,
        List<DepartmentId> departmentIds,
        CancellationToken cancellationToken)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var positionId = new PositionId(Guid.NewGuid());
            var position = Position.Create(
                PositionName.Create(name).Value,
                null,
                departmentIds.Select(departmentId => new DepartmentPosition(departmentId, positionId)).ToList(),
                positionId);

            await dbContext.Positions.AddAsync(position.Value, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return positionId;
        });
    }
}