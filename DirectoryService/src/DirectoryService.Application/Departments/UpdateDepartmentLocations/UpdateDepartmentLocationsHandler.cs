using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.UpdateDepartmentLocations;

public class UpdateDepartmentLocationsHandler : ICommandHandler<Guid, UpdateDepartmentLocationsCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdateDepartmentLocationsCommand> _validator;
    private readonly ILogger<UpdateDepartmentLocationsHandler> _logger;

    public UpdateDepartmentLocationsHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        IValidator<UpdateDepartmentLocationsCommand> validator,
        ILogger<UpdateDepartmentLocationsHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        UpdateDepartmentLocationsCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        if (!await _departmentRepository.IsActiveLocationsExistAsync(
                command.Request.LocationIds.Select(locId => new LocationId(locId)).ToList(),
                cancellationToken))
        {
            return Error.NotFound(
                "location.not.found",
                $"В базе данных отсутствуют одна или несколько локаций из списка").ToErrors();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error.ToErrors();
        }

        // Использование using ОБЯЗАТЕЛЬНО, так как это гарантирует, что Dispose будет вызван всегда, даже при исключении
        using var transactionScope = transactionScopeResult.Value;

        var department = await _departmentRepository
            .GetByIdAsync(new DepartmentId(command.DepartmentId), cancellationToken);
        if (department.IsFailure)
        {
            transactionScope.Rollback();
            return department.Error.ToErrors();
        }

        if (!department.Value.IsActive)
        {
            transactionScope.Rollback();
            return Error.NotFound(
                "department.not.active",
                $"Родительское подразделение с идентификатором {command.DepartmentId} не активно").ToErrors();
        }

        var departmentLocations = command.Request.LocationIds
            .Select(locId => new DepartmentLocation(new DepartmentId(command.DepartmentId), new LocationId(locId)))
            .ToList();

        department.Value.UpdateLocations(departmentLocations);

        var deleteResult = await _departmentRepository.
            DeleteDepartmentLocationsByIdAsync(department.Value.Id, cancellationToken);

        if (deleteResult.IsFailure)
        {
            transactionScope.Rollback();
            return deleteResult.Error.ToErrors();
        }

        var saveChangeResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangeResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveChangeResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();

        if (commitResult.IsSuccess)
        {
            _logger.LogInformation(
                "У подразделения {DepartmentName} (id {DepartmentId}) локации успешно обновлены",
                department.Value.Name.Value,
                department.Value.Id.Value);

            return department.Value.Id.Value;
        }
        else
        {
            return commitResult.Error.ToErrors();
        }
    }
}