using System.Data;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.UpdateDepartmentLocations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.UpdateDepartmentParent;

public class UpdateDepartmentParentHandler : ICommandHandler<Guid, UpdateDepartmentParentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdateDepartmentParentCommand> _validator;
    private readonly ILogger<UpdateDepartmentParentHandler> _logger;

    public UpdateDepartmentParentHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        IValidator<UpdateDepartmentParentCommand> validator,
        ILogger<UpdateDepartmentParentHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        UpdateDepartmentParentCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        if (command.DepartmentId == command.Request.ParentId)
        {
            return Error.Validation(
                "value.is.required",
                "У департамента и родителя указаны один и тот же ID")
                .ToErrors();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(
            cancellationToken,
            IsolationLevel.RepeatableRead);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error.ToErrors();
        }

        // Использование using ОБЯЗАТЕЛЬНО, так как это гарантирует, что Dispose будет вызван всегда, даже при исключении
        using var transactionScope = transactionScopeResult.Value;

        // Проверить, что существует подразделение с таким departmentId и оно активно
        var department = await _departmentRepository
            .GetByIdWithLockAsync(new DepartmentId(command.DepartmentId), cancellationToken);

        if (department.IsFailure)
        {
            transactionScope.Rollback();
            return department.Error.ToErrors();
        }

        // Проверить, что новый parentId (если не null) существует, активен и не совпадает с departmentId
        Department? parent;
        if (command.Request.ParentId == null)
        {
            parent = null;
        }
        else
        {
            var newParent = await _departmentRepository
                .GetByIdWithLockAsync(new DepartmentId(command.Request.ParentId.Value), cancellationToken);

            if (newParent.IsFailure)
            {
                transactionScope.Rollback();
                return newParent.Error.ToErrors();
            }

            parent = newParent.Value;

            // Нельзя выбрать родителем своё "дочернее" подразделение (чтобы не было зацикливания структуры)
            var isParent = await _departmentRepository.IsParent(
                department.Value.Path,
                parent.Id,
                cancellationToken);
            if (isParent.IsFailure)
            {
                transactionScope.Rollback();
                return isParent.Error.ToErrors();
            }

            if (isParent.Value)
            {
                transactionScope.Rollback();
                return Error.Conflict(
                        "parent.is.conflict",
                        "В качестве родителя выбрано своё \"дочернее\" подразделение")
                    .ToErrors();
            }
        }

        // Блокировка подчинённых подразделений для дальнейшего массового обновления
        string oldPath = department.Value.Path;
        var lockDescendantsResult = await _departmentRepository.LockDescendantsAsync(oldPath, cancellationToken);
        if (lockDescendantsResult.IsFailure)
        {
            transactionScope.Rollback();
            return lockDescendantsResult.Error.ToErrors();
        }

        // Изменить parentId у подразделения, пересчитать и обновить Path, Depth
        department.Value.UpdateParent(parent);
        var saveChangeResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangeResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveChangeResult.Error.ToErrors();
        }

        // Для всех дочерних подразделений и их потомков обновить Path и Depth, использовать Ltree
        var updateResult = await _departmentRepository.UpdateDescendantProperties(
            oldPath, department.Value.Path, cancellationToken);
        if (updateResult.IsFailure)
        {
            transactionScope.Rollback();
            return updateResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsSuccess)
        {
            _logger.LogInformation(
                "У подразделения {DepartmentName} (id {DepartmentId}) изменено родительское подразделение. Данные успешно обновлены.",
                department.Value.Name.Value,
                department.Value.Id.Value);
            return department.Value.Id.Value;
        }

        transactionScope.Rollback();
        return commitResult.Error.ToErrors();
    }
}