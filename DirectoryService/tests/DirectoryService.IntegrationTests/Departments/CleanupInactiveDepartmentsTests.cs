using DirectoryService.Application.Departments.SoftDeleteDepartment;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class CleanupInactiveDepartmentsTests : DirectoryBaseTests
{
    public CleanupInactiveDepartmentsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CleanupExpiredDepartments_should_delete_department_and_rebuild_paths()
    {
        var cancellationToken = CancellationToken.None;
        var departments = await CreateDepartmentBranchAsync(cancellationToken);
        var root = departments[0];
        var deletedDepartment = departments[1];
        var child = departments[2];
        var grandChild = departments[3];
        var positionId = await CreatePositionAsync("TeamLead", [deletedDepartment.Id], cancellationToken);

        var softDeleteResult = await ExecuteSoftDeleteHandler(sut =>
            sut.Handle(new SoftDeleteDepartmentCommand(deletedDepartment.Id.Value), cancellationToken));

        await ExecuteInDb(async dbContext =>
        {
            await dbContext.Database.ExecuteSqlAsync(
                $"""
                 UPDATE departments
                 SET deleted_at = {DateTime.UtcNow.AddMonths(-2)}
                 WHERE department_id = {deletedDepartment.Id.Value};
                 """,
                cancellationToken);
        });

        int deletedCount = await ExecuteCleanupService(sut => sut.CleanupExpiredDepartmentsAsync(cancellationToken));

        await ExecuteInDb(async dbContext =>
        {
            var deletedEntity = await dbContext.Departments
                .SingleOrDefaultAsync(d => d.Id == deletedDepartment.Id, cancellationToken);
            var actualRoot = await dbContext.Departments
                .SingleAsync(d => d.Id == root.Id, cancellationToken);
            var actualChild = await dbContext.Departments
                .SingleAsync(d => d.Id == child.Id, cancellationToken);
            var actualGrandChild = await dbContext.Departments
                .SingleAsync(d => d.Id == grandChild.Id, cancellationToken);

            int departmentLocationsCount = await dbContext.DepartmentLocations
                .CountAsync(dl => dl.DepartmentId == deletedDepartment.Id, cancellationToken);
            int departmentPositionsCount = await dbContext.DepartmentPositions
                .CountAsync(dp => dp.DepartmentId == deletedDepartment.Id, cancellationToken);
            bool positionExists = await dbContext.Positions
                .AnyAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(softDeleteResult.IsSuccess);
            Assert.Equal(1, deletedCount);
            Assert.Null(deletedEntity);
            Assert.Equal(root.Id, actualChild.ParentId);
            Assert.Equal($"{actualRoot.Path}.{child.Identifier.Value}", actualChild.Path);
            Assert.Equal($"{actualChild.Path}.{grandChild.Identifier.Value}", actualGrandChild.Path);
            Assert.Equal(actualChild.Depth + 1, actualGrandChild.Depth);
            Assert.Equal(0, departmentLocationsCount);
            Assert.Equal(0, departmentPositionsCount);
            Assert.True(positionExists);
        });
    }

    [Fact]
    public async Task CleanupExpiredDepartments_should_skip_department_deleted_less_than_month_ago()
    {
        var cancellationToken = CancellationToken.None;
        var departments = await CreateDepartmentBranchAsync(cancellationToken);
        var deletedDepartment = departments[1];

        var softDeleteResult = await ExecuteSoftDeleteHandler(sut =>
            sut.Handle(new SoftDeleteDepartmentCommand(deletedDepartment.Id.Value), cancellationToken));

        int deletedCount = await ExecuteCleanupService(sut => sut.CleanupExpiredDepartmentsAsync(cancellationToken));

        await ExecuteInDb(async dbContext =>
        {
            var actualDepartment = await dbContext.Departments
                .SingleOrDefaultAsync(d => d.Id == deletedDepartment.Id, cancellationToken);

            Assert.True(softDeleteResult.IsSuccess);
            Assert.Equal(0, deletedCount);
            Assert.NotNull(actualDepartment);
            Assert.False(actualDepartment.IsActive);
        });
    }

    private async Task<T> ExecuteSoftDeleteHandler<T>(Func<SoftDeleteDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<SoftDeleteDepartmentHandler>();

        return await action(sut);
    }

    private async Task<T> ExecuteCleanupService<T>(Func<IDepartmentCleanupService, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<IDepartmentCleanupService>();

        return await action(sut);
    }

    private async Task<List<Department>> CreateDepartmentBranchAsync(CancellationToken cancellationToken)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var rootLocation = await CreateLocationAsync(cancellationToken);
            var departmentLocation = await CreateLocationAsync(cancellationToken);
            var childLocation = await CreateLocationAsync(cancellationToken);
            var grandChildLocation = await CreateLocationAsync(cancellationToken);

            var rootId = new DepartmentId(Guid.NewGuid());
            var root = Department.CreateParent(
                DepartmentName.Create("Headquarters").Value,
                Identifier.Create("hq").Value,
                [new DepartmentLocation(rootId, rootLocation)],
                rootId).Value;

            var departmentId = new DepartmentId(Guid.NewGuid());
            var department = Department.CreateChild(
                DepartmentName.Create("Information Technology").Value,
                Identifier.Create("it").Value,
                root,
                [new DepartmentLocation(departmentId, departmentLocation)],
                departmentId).Value;

            var childId = new DepartmentId(Guid.NewGuid());
            var child = Department.CreateChild(
                DepartmentName.Create("Development").Value,
                Identifier.Create("dev_team").Value,
                department,
                [new DepartmentLocation(childId, childLocation)],
                childId).Value;

            var grandChildId = new DepartmentId(Guid.NewGuid());
            var grandChild = Department.CreateChild(
                DepartmentName.Create("Platform").Value,
                Identifier.Create("platform").Value,
                child,
                [new DepartmentLocation(grandChildId, grandChildLocation)],
                grandChildId).Value;

            await dbContext.Departments.AddRangeAsync([root, department, child, grandChild], cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new List<Department> { root, department, child, grandChild };
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