using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.SoftDeleteDepartment;

public class SoftDeleteDepartmentHandler : ICommandHandler<Guid, SoftDeleteDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<SoftDeleteDepartmentCommand> _validator;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SoftDeleteDepartmentHandler> _logger;

    public SoftDeleteDepartmentHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        IValidator<SoftDeleteDepartmentCommand> validator,
        ICacheService cacheService,
        ILogger<SoftDeleteDepartmentHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(SoftDeleteDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error.ToErrors();
        }

        // Использование using ОБЯЗАТЕЛЬНО, так как это гарантирует, что Dispose будет вызван всегда, даже при исключении
        using var transactionScope = transactionScopeResult.Value;

        var department = await _departmentRepository
            .GetByIdWithLockAsync(new DepartmentId(command.DepartmentId), cancellationToken);

        if (department.IsFailure)
        {
            transactionScope.Rollback();
            return department.Error.ToErrors();
        }

        string oldPath = department.Value.Path;

        // Блокировка подчинённых подразделений для дальнейшего массового обновления
        var lockDescendantsResult = await _departmentRepository.LockDescendantsAsync(oldPath, cancellationToken);
        if (lockDescendantsResult.IsFailure)
        {
            transactionScope.Rollback();
            return lockDescendantsResult.Error.ToErrors();
        }

        department.Value.SoftDelete();
        var saveChangeResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangeResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveChangeResult.Error.ToErrors();
        }

        // деактивация связанных локаций
        var softDeleteDepartmentLocationsResult = await _departmentRepository
            .SoftDeleteDepartmentLocations(command.DepartmentId, cancellationToken);
        if (softDeleteDepartmentLocationsResult.IsFailure)
        {
            transactionScope.Rollback();
            return softDeleteDepartmentLocationsResult.Error.ToErrors();
        }

        // деактивация связанных должностей
        var softDeleteDepartmentPositionsResult = await _departmentRepository
            .SoftDeleteDepartmentPositions(command.DepartmentId, cancellationToken);
        if (softDeleteDepartmentPositionsResult.IsFailure)
        {
            transactionScope.Rollback();
            return softDeleteDepartmentPositionsResult.Error.ToErrors();
        }

        // обновление путей у дочерних подразделений
        string newPath = department.Value.Path;
        var updateSubPathsResult = await _departmentRepository.UpdateSubPaths(
            oldPath, newPath, cancellationToken);
        if (updateSubPathsResult.IsFailure)
        {
            transactionScope.Rollback();
            return updateSubPathsResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsSuccess)
        {
            await _cacheService.RemoveByTagAsync(DepartmentsCache.Tag, cancellationToken);

            _logger.LogInformation(
                "Подразделение {DepartmentName} (id {DepartmentId}) деактивировано. Данные успешно обновлены.",
                department.Value.Name.Value,
                department.Value.Id.Value);
            return department.Value.Id.Value;
        }

        transactionScope.Rollback();
        return commitResult.Error.ToErrors();
    }
}