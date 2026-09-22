using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core.Abstractions;
using Shared.Core.Validation;
using Shared.Kernel;

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

        Department department;
        await using (ITransactionScope transactionScope = transactionScopeResult.Value)
        {
            var departmentResult = await _departmentRepository
                .GetByIdWithLockAsync(new DepartmentId(command.DepartmentId), cancellationToken);

            if (departmentResult.IsFailure)
            {
                return departmentResult.Error.ToErrors();
            }

            department = departmentResult.Value;
            string oldPath = department.Path;

            // Блокировка подчинённых подразделений для дальнейшего массового обновления
            var lockDescendantsResult = await _departmentRepository.LockDescendantsAsync(oldPath, cancellationToken);
            if (lockDescendantsResult.IsFailure)
            {
                return lockDescendantsResult.Error.ToErrors();
            }

            department.SoftDelete();
            var saveChangeResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveChangeResult.IsFailure)
            {
                return saveChangeResult.Error.ToErrors();
            }

            // деактивация связанных локаций
            var softDeleteDepartmentLocationsResult = await _departmentRepository
                .SoftDeleteDepartmentLocations(command.DepartmentId, cancellationToken);
            if (softDeleteDepartmentLocationsResult.IsFailure)
            {
                return softDeleteDepartmentLocationsResult.Error.ToErrors();
            }

            // деактивация связанных должностей
            var softDeleteDepartmentPositionsResult = await _departmentRepository
                .SoftDeleteDepartmentPositions(command.DepartmentId, cancellationToken);
            if (softDeleteDepartmentPositionsResult.IsFailure)
            {
                return softDeleteDepartmentPositionsResult.Error.ToErrors();
            }

            // обновление путей у дочерних подразделений
            string newPath = department.Path;
            var updateSubPathsResult = await _departmentRepository.UpdateSubPaths(
                oldPath, newPath, cancellationToken);
            if (updateSubPathsResult.IsFailure)
            {
                return updateSubPathsResult.Error.ToErrors();
            }

            var commitResult = await transactionScope.CommitAsync(cancellationToken);
            if (commitResult.IsFailure)
            {
                return commitResult.Error.ToErrors();
            }
        }

        await _cacheService.RemoveByTagAsync(DepartmentsCache.Tag, CancellationToken.None);

        _logger.LogInformation(
            "Подразделение {DepartmentName} (id {DepartmentId}) деактивировано. Данные успешно обновлены.",
            department.Name.Value,
            department.Id.Value);

        return department.Id.Value;
    }
}