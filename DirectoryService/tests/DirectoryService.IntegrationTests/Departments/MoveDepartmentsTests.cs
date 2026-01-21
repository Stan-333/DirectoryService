using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.UpdateDepartmentParent;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class MoveDepartmentsTests : DirectoryBaseTests
{
    public MoveDepartmentsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task MoveDepartment_down_with_valid_data_should_success()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var departments = await CreateDepartmentTree(cancellationToken);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentParentCommand(
                departments[3].Id.Value,
                new UpdateDepartmentParentRequest(departments[8].Id.Value));

            return sut.Handle(command, cancellationToken);
        });

        // Assert
        await ExecuteInDb(async dbContext =>
        {
            var department3 = await dbContext.Departments
                .Include(d => d.Parent)
                .SingleAsync(d => d.Id == departments[3].Id, cancellationToken);
            var department8 = await dbContext.Departments
                .SingleAsync(d => d.Id == departments[8].Id, cancellationToken);
            var department4 = await dbContext.Departments
                .SingleAsync(d => d.Id == departments[4].Id, cancellationToken);
            var department7 = await dbContext.Departments
                .SingleAsync(d => d.Id == departments[7].Id, cancellationToken);

            Assert.NotNull(department3.Parent);
            Assert.Equal(department3.Parent, department8);
            Assert.Equal(department3.Path, $"{department8.Path}.{department3.Identifier.Value}");
            Assert.Equal(
                department7.Path,
                $"{department8.Path}.{department3.Identifier.Value}.{department4.Identifier.Value}.{department7.Identifier.Value}");
            Assert.Equal(department3.Depth, department8.Depth + 1);
            Assert.Equal(department7.Depth, department8.Depth + 3);

            Assert.True(result.IsSuccess);
        });
    }

    [Fact]
    public async Task MoveDepartment_up_with_valid_data_should_success()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var departments = await CreateDepartmentTree(cancellationToken);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentParentCommand(
                departments[3].Id.Value,
                new UpdateDepartmentParentRequest(departments[0].Id.Value));

            return sut.Handle(command, cancellationToken);
        });

        // Assert
        await ExecuteInDb(async dbContext =>
        {
            var department3 = await dbContext.Departments
                .Include(d => d.Parent)
                .SingleAsync(d => d.Id == departments[3].Id, cancellationToken);
            var department0 = await dbContext.Departments
                .SingleAsync(d => d.Id == departments[0].Id, cancellationToken);
            var department4 = await dbContext.Departments
                .SingleAsync(d => d.Id == departments[4].Id, cancellationToken);
            var department7 = await dbContext.Departments
                .SingleAsync(d => d.Id == departments[7].Id, cancellationToken);

            Assert.NotNull(department3.Parent);
            Assert.Equal(department3.Parent, department0);
            Assert.Equal(department3.Path, $"{department0.Path}.{department3.Identifier.Value}");
            Assert.Equal(
                department7.Path,
                $"{department0.Path}.{department3.Identifier.Value}.{department4.Identifier.Value}.{department7.Identifier.Value}");
            Assert.Equal(department3.Depth, department0.Depth + 1);
            Assert.Equal(department7.Depth, department0.Depth + 3);

            Assert.True(result.IsSuccess);
        });
    }

    [Fact]
    public async Task MoveDepartment_to_children_should_failure()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var departments = await CreateDepartmentTree(cancellationToken);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentParentCommand(
                departments[3].Id.Value,
                new UpdateDepartmentParentRequest(departments[6].Id.Value));

            return sut.Handle(command, cancellationToken);
        });

        // Assert
        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<UpdateDepartmentParentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<UpdateDepartmentParentHandler>();

        return await action(sut);
    }

    private async Task<List<Department>> CreateDepartmentTree(CancellationToken cancellationToken)
    {
        List<Department> departments = [];
        await ExecuteInDb(async dbContext =>
        {
            var locations0 = await CreateManyLocationsAsync(3, cancellationToken);
            var departmentId0 = new DepartmentId(Guid.NewGuid());
            var department0 = Department.CreateParent(
                DepartmentName.Create("Д-0").Value,
                Identifier.Create("zero").Value,
                locations0.Select(locId => new DepartmentLocation(departmentId0, locId)).ToList(),
                departmentId0);
            departments.Add(department0.Value);

            var locationId1 = await CreateLocationAsync(cancellationToken);
            var departmentId1 = new DepartmentId(Guid.NewGuid());
            var department1 = Department.CreateChild(
                DepartmentName.Create("Д-1").Value,
                Identifier.Create("one").Value,
                department0.Value,
                [new DepartmentLocation(departmentId1, locationId1)],
                departmentId1);
            departments.Add(department1.Value);

            var locationId2 = await CreateLocationAsync(cancellationToken);
            var departmentId2 = new DepartmentId(Guid.NewGuid());
            var department2 = Department.CreateChild(
                DepartmentName.Create("Д-2").Value,
                Identifier.Create("two").Value,
                department1.Value,
                [new DepartmentLocation(departmentId2, locationId2)],
                departmentId2);
            departments.Add(department2.Value);

            var locations3 = await CreateManyLocationsAsync(5, cancellationToken);
            var departmentId3 = new DepartmentId(Guid.NewGuid());
            var department3 = Department.CreateChild(
                DepartmentName.Create("Д-3").Value,
                Identifier.Create("three").Value,
                department1.Value,
                locations3.Select(locId => new DepartmentLocation(departmentId3, locId)).ToList(),
                departmentId3);
            departments.Add(department3.Value);

            var locationId4 = await CreateLocationAsync(cancellationToken);
            var departmentId4 = new DepartmentId(Guid.NewGuid());
            var department4 = Department.CreateChild(
                DepartmentName.Create("Д-4").Value,
                Identifier.Create("four").Value,
                department3.Value,
                [new DepartmentLocation(departmentId4, locationId4)],
                departmentId4);
            departments.Add(department4.Value);

            var locationId5 = await CreateLocationAsync(cancellationToken);
            var departmentId5 = new DepartmentId(Guid.NewGuid());
            var department5 = Department.CreateChild(
                DepartmentName.Create("Д-5").Value,
                Identifier.Create("five").Value,
                department3.Value,
                [new DepartmentLocation(departmentId5, locationId5)],
                departmentId5);
            departments.Add(department5.Value);

            var locationId6 = await CreateLocationAsync(cancellationToken);
            var departmentId6 = new DepartmentId(Guid.NewGuid());
            var department6 = Department.CreateChild(
                DepartmentName.Create("Д-6").Value,
                Identifier.Create("six").Value,
                department4.Value,
                [new DepartmentLocation(departmentId6, locationId6)],
                departmentId6);
            departments.Add(department6.Value);

            var locationId7 = await CreateLocationAsync(cancellationToken);
            var departmentId7 = new DepartmentId(Guid.NewGuid());
            var department7 = Department.CreateChild(
                DepartmentName.Create("Д-7").Value,
                Identifier.Create("seven").Value,
                department4.Value,
                [new DepartmentLocation(departmentId7, locationId7)],
                departmentId7);
            departments.Add(department7.Value);

            var locationId8 = await CreateLocationAsync(cancellationToken);
            var departmentId8 = new DepartmentId(Guid.NewGuid());
            var department8 = Department.CreateChild(
                DepartmentName.Create("Д-8").Value,
                Identifier.Create("eight").Value,
                department2.Value,
                [new DepartmentLocation(departmentId8, locationId8)],
                departmentId8);
            departments.Add(department8.Value);

            var locationId9 = await CreateLocationAsync(cancellationToken);
            var departmentId9 = new DepartmentId(Guid.NewGuid());
            var department9 = Department.CreateChild(
                DepartmentName.Create("Д-9").Value,
                Identifier.Create("nine").Value,
                department2.Value,
                [new DepartmentLocation(departmentId9, locationId9)],
                departmentId9);
            departments.Add(department9.Value);

            await dbContext.Departments.AddRangeAsync(departments, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        });

        return departments;
    }
}