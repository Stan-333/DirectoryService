using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
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
    private readonly ICacheService _cacheService;
    private readonly ILogger<UpdateDepartmentLocationsHandler> _logger;

    public UpdateDepartmentLocationsHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        IValidator<UpdateDepartmentLocationsCommand> validator,
        ICacheService cacheService,
        ILogger<UpdateDepartmentLocationsHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _cacheService = cacheService;
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

            var departmentLocations = command.Request.LocationIds
                .Select(locId => new DepartmentLocation(new DepartmentId(command.DepartmentId), new LocationId(locId)))
                .ToList();

            department.UpdateLocations(departmentLocations);

            var deleteResult = await _departmentRepository.
                DeleteDepartmentLocationsByIdAsync(department.Id, cancellationToken);

            if (deleteResult.IsFailure)
            {
                return deleteResult.Error.ToErrors();
            }

            var saveChangeResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveChangeResult.IsFailure)
            {
                return saveChangeResult.Error.ToErrors();
            }

            var commitResult = await transactionScope.CommitAsync(cancellationToken);
            if (commitResult.IsFailure)
            {
                return commitResult.Error.ToErrors();
            }
        }

        await _cacheService.RemoveByTagAsync(DepartmentsCache.Tag, CancellationToken.None);

        _logger.LogInformation(
            "У подразделения {DepartmentName} (id {DepartmentId}) локации успешно обновлены",
            department.Name.Value,
            department.Id.Value);

        return department.Id.Value;
    }
}