using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Domain.Departments;
using DirectoryService.Infrastructure;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTests : DirectoryBaseTests
{
    public CreateDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateDepartment_with_valid_data_should_success()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationAsync(cancellationToken);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Подразделение",
                    "podrazdelenie",
                    null,
                    [locationId.Value]));

            return sut.Handle(command, cancellationToken);
        });

        // Assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FindAsync([new DepartmentId(result.Value)], cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(department.Id.Value, result.Value);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_many_locations_should_success()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var locationIds = await CreateManyLocationsAsync(200, cancellationToken);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Подразделение",
                    "podrazdelenie",
                    null,
                    locationIds.Select(id => id.Value).ToList()));

            return sut.Handle(command, cancellationToken);
        });

        // Assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FindAsync([new DepartmentId(result.Value)], cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(department.Id.Value, result.Value);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_invalid_data_should_fail()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationAsync(cancellationToken);

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Подразделение-2", "2ывцК%3", null, [locationId.Value]));

            return sut.Handle(command, cancellationToken);
        });

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_duplicate_identifier_should_return_conflict()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var locationId = await CreateLocationAsync(cancellationToken);

        var first = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Подразделение", "duplicate", null, [locationId.Value]));

            return sut.Handle(command, cancellationToken);
        });

        Assert.True(first.IsSuccess);

        // Act
        var second = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Другое подразделение", "duplicate", null, [locationId.Value]));

            return sut.Handle(command, cancellationToken);
        });

        // Assert
        Assert.True(second.IsFailure);
        Assert.Contains(second.Error, e => e.Type == ErrorType.CONFLICT);
    }

    [Fact]
    public async Task CreateChildDepartment_when_parent_soft_deleted_while_waiting_for_lock_should_fail()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var parentLocationId = await CreateLocationAsync(cancellationToken);
        var childLocationId = await CreateLocationAsync(cancellationToken);

        var createParentResult = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Родитель",
                    "parent",
                    null,
                    [parentLocationId.Value]));

            return sut.Handle(command, cancellationToken);
        });

        Assert.True(createParentResult.IsSuccess);
        var parentId = new DepartmentId(createParentResult.Value);

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM departments WHERE department_id = {parentId.Value} FOR UPDATE;",
            cancellationToken);

        var createChildTask = ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Дочерний",
                    "child",
                    parentId.Value,
                    [childLocationId.Value]));

            return sut.Handle(command, cancellationToken);
        });

        var completedTask = await Task.WhenAny(createChildTask, Task.Delay(200, cancellationToken));
        Assert.NotSame(createChildTask, completedTask);

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
             UPDATE departments
             SET is_active = FALSE,
                 deleted_at = now(),
                 updated_at = now()
             WHERE department_id = {parentId.Value};
             """,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        var result = await createChildTask;

        // Assert
        Assert.True(result.IsFailure);

        await ExecuteInDb(async context =>
        {
            var childDepartment = await context.Departments
                .SingleOrDefaultAsync(d => d.Identifier == Identifier.Create("child").Value, cancellationToken);

            Assert.Null(childDepartment);
        });
    }

    private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

        return await action(sut);
    }
}