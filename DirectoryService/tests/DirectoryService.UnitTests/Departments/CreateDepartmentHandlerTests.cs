using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shared.Core.Abstractions;
using Shared.Kernel;

namespace DirectoryService.UnitTests.Departments;

public class CreateDepartmentHandlerTests
{
    [Fact]
    public async Task Handle_child_should_dispose_transaction_before_invalidating_cache()
    {
        // Arrange
        var repository = Substitute.For<IDepartmentRepository>();
        var transactionManager = Substitute.For<ITransactionManager>();
        var transactionScope = new TrackingTransactionScope();
        var cache = new TrackingCacheService(() => transactionScope.IsDisposed);
        var validator = new CreateDepartmentCommandValidator();

        LocationId locationId = new LocationId(Guid.NewGuid());
        DepartmentId parentId = new DepartmentId(Guid.NewGuid());
        Department parent = Department.CreateParent(
            DepartmentName.Create("Родитель").Value,
            Identifier.Create("parent").Value,
            [new DepartmentLocation(parentId, locationId)],
            parentId).Value;

        repository.IsActiveIdentifierExistAsync(Arg.Any<Identifier>(), Arg.Any<CancellationToken>())
            .Returns(false);
        repository.IsActiveLocationsExistAsync(Arg.Any<List<LocationId>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        repository.GetByIdWithLockAsync(Arg.Any<DepartmentId>(), Arg.Any<CancellationToken>())
            .Returns(parent);
        repository.AddAsync(Arg.Any<Department>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Department>().Id.Value);

        transactionManager.BeginTransactionAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success<ITransactionScope, Error>(transactionScope));
        transactionManager.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<Error>());

        var sut = new CreateDepartmentHandler(
            repository,
            transactionManager,
            validator,
            cache,
            NullLogger<CreateDepartmentHandler>.Instance);
        var command = new CreateDepartmentCommand(
            new CreateDepartmentRequest(
                "Дочернее подразделение",
                "child",
                parent.Id.Value,
                [locationId.Value]));

        // Act
        Result<Guid, Errors> result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(transactionScope.IsCommitted);
        Assert.True(transactionScope.IsDisposed);
        Assert.False(transactionScope.IsRolledBackExplicitly);
        Assert.True(cache.WasTransactionDisposedDuringInvalidation);
        Assert.Equal(CancellationToken.None, cache.InvalidationToken);
    }

    [Fact]
    public async Task Handle_child_when_parent_missing_should_dispose_uncommitted_transaction()
    {
        // Arrange
        var repository = Substitute.For<IDepartmentRepository>();
        var transactionManager = Substitute.For<ITransactionManager>();
        var transactionScope = new TrackingTransactionScope();
        var cache = new TrackingCacheService(() => transactionScope.IsDisposed);

        repository.IsActiveIdentifierExistAsync(Arg.Any<Identifier>(), Arg.Any<CancellationToken>())
            .Returns(false);
        repository.IsActiveLocationsExistAsync(Arg.Any<List<LocationId>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        repository.GetByIdWithLockAsync(Arg.Any<DepartmentId>(), Arg.Any<CancellationToken>())
            .Returns(GeneralErrors.NotFound(Guid.NewGuid(), nameof(Department)));

        transactionManager.BeginTransactionAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success<ITransactionScope, Error>(transactionScope));

        var sut = new CreateDepartmentHandler(
            repository,
            transactionManager,
            new CreateDepartmentCommandValidator(),
            cache,
            NullLogger<CreateDepartmentHandler>.Instance);
        var command = new CreateDepartmentCommand(
            new CreateDepartmentRequest(
                "Дочернее подразделение",
                "child",
                Guid.NewGuid(),
                [Guid.NewGuid()]));

        // Act
        Result<Guid, Errors> result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.False(transactionScope.IsCommitted);
        Assert.True(transactionScope.IsDisposed);
        Assert.False(transactionScope.IsRolledBackExplicitly);
        Assert.Equal(0, cache.InvalidationCount);
    }

    private sealed class TrackingTransactionScope : ITransactionScope
    {
        public bool IsCommitted { get; private set; }

        public bool IsDisposed { get; private set; }

        public bool IsRolledBackExplicitly { get; private set; }

        public Task<UnitResult<Error>> CommitAsync(CancellationToken cancellationToken = default)
        {
            IsCommitted = true;
            return Task.FromResult(UnitResult.Success<Error>());
        }

        public Task<UnitResult<Error>> RollbackAsync(CancellationToken cancellationToken = default)
        {
            IsRolledBackExplicitly = true;
            return Task.FromResult(UnitResult.Success<Error>());
        }

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class TrackingCacheService : ICacheService
    {
        private readonly Func<bool> _isTransactionDisposed;

        public TrackingCacheService(Func<bool> isTransactionDisposed)
        {
            _isTransactionDisposed = isTransactionDisposed;
        }

        public bool WasTransactionDisposedDuringInvalidation { get; private set; }

        public CancellationToken InvalidationToken { get; private set; }

        public int InvalidationCount { get; private set; }

        public ValueTask<T> GetOrCreateAsync<T>(
            string key,
            Func<CancellationToken, ValueTask<T>> factory,
            TimeSpan? ttl = null,
            IReadOnlyList<string>? tags = null,
            CancellationToken cancellationToken = default)
        {
            return factory(cancellationToken);
        }

        public ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
        {
            InvalidationCount++;
            WasTransactionDisposedDuringInvalidation = _isTransactionDisposed();
            InvalidationToken = cancellationToken;
            return ValueTask.CompletedTask;
        }
    }
}