using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.UpdateDepartmentLocations;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateDepartmentLocationTests : DirectoryBaseTests
{
    public UpdateDepartmentLocationTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_valid_data_should_success()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var departmentId = new DepartmentId(Guid.NewGuid());
        var oldLocationIds = await CreateManyLocationsAsync(5, cancellationToken);
        var newLocationIds = await CreateManyLocationsAsync(4, cancellationToken);
        await ExecuteInDb(async dbContext =>
        {
            var department = Department.CreateParent(
                DepartmentName.Create("Д-0").Value,
                Identifier.Create("zero").Value,
                oldLocationIds.Select(locId => new DepartmentLocation(departmentId, locId)).ToList(),
                departmentId);

            await dbContext.Departments.AddAsync(department.Value, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        });

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentLocationsCommand(
                departmentId.Value,
                new UpdateDepartmentLocationsRequest(newLocationIds.Select(id => id.Value).ToList()));

            return sut.Handle(command, cancellationToken);
        });

        // Assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .SingleAsync(d => d.Id == departmentId, cancellationToken);

            Assert.Equal(
                department.DepartmentLocations.Select(dl => dl.LocationId.Value).ToList().OrderBy(l => l),
                newLocationIds.Select(l => l.Value).ToList().OrderBy(l => l));
        });

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_invalid_location_data_should_fail()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var departmentId = new DepartmentId(Guid.NewGuid());
        var oldLocationIds = await CreateManyLocationsAsync(5, cancellationToken);
        var newLocationIds = await CreateManyLocationsAsync(2, cancellationToken);
        newLocationIds.Add(new LocationId(Guid.NewGuid()));
        await ExecuteInDb(async dbContext =>
        {
            var department = Department.CreateParent(
                DepartmentName.Create("Д-0").Value,
                Identifier.Create("zero").Value,
                oldLocationIds.Select(locId => new DepartmentLocation(departmentId, locId)).ToList(),
                departmentId);

            await dbContext.Departments.AddAsync(department.Value, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        });

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentLocationsCommand(
                departmentId.Value,
                new UpdateDepartmentLocationsRequest(newLocationIds.Select(id => id.Value).ToList()));

            return sut.Handle(command, cancellationToken);
        });

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task UpdateDepartmentLocations_with_duplicate_location_data_should_fail()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var departmentId = new DepartmentId(Guid.NewGuid());
        var oldLocationIds = await CreateManyLocationsAsync(5, cancellationToken);
        var newLocationIds = await CreateManyLocationsAsync(3, cancellationToken);
        newLocationIds.Add(newLocationIds[1]);
        await ExecuteInDb(async dbContext =>
        {
            var department = Department.CreateParent(
                DepartmentName.Create("Д-0").Value,
                Identifier.Create("zero").Value,
                oldLocationIds.Select(locId => new DepartmentLocation(departmentId, locId)).ToList(),
                departmentId);

            await dbContext.Departments.AddAsync(department.Value, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        });

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentLocationsCommand(
                departmentId.Value,
                new UpdateDepartmentLocationsRequest(newLocationIds.Select(id => id.Value).ToList()));

            return sut.Handle(command, cancellationToken);
        });

        // Assert
        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteHandler<T>(Func<UpdateDepartmentLocationsHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<UpdateDepartmentLocationsHandler>();

        return await action(sut);
    }
}