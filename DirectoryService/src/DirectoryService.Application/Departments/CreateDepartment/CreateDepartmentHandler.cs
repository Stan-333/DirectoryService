using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly ILogger<CreateDepartmentHandler> _logger;

    public CreateDepartmentHandler(
        IDepartmentRepository departmentRepository,
        IValidator<CreateDepartmentCommand> validator,
        ILogger<CreateDepartmentHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        Identifier identifier = Identifier.Create(command.Request.Identifier).Value;
        if (await _departmentRepository.IsActiveIdentifierExistAsync(identifier, cancellationToken))
        {
            return Error.Validation(
                "record.already.exist",
                "Подразделение с таким идентификатором уже существует")
                .ToErrors();
        }

        foreach (Guid guid in command.Request.LocationIds)
        {
            if (!await _departmentRepository.IsActiveLocationExistAsync(new LocationId(guid), cancellationToken))
            {
                return Error.NotFound(
                    "location.not.found",
                    $"Локация с id - {guid} отсутствует",
                    guid).ToErrors();
            }
        }

        DepartmentId departmentId = new DepartmentId(Guid.NewGuid());
        var departmentLocations = command.Request.LocationIds
            .Select(locId => new DepartmentLocation(departmentId, new LocationId(locId))).ToList();

        DepartmentName departmentName = DepartmentName.Create(command.Request.Name).Value;
        Result<Department, Error> department;
        if (command.Request.ParentId is null)
        {
            department = Department.CreateParent(
                departmentName,
                identifier,
                departmentLocations,
                departmentId);
        }
        else
        {
            var parent = await _departmentRepository.GetByIdAsync(
                new DepartmentId(command.Request.ParentId.Value), cancellationToken);

            if (parent.IsFailure)
            {
                return parent.Error.ToErrors();
            }

            department = Department.CreateChild(
                departmentName,
                identifier,
                parent.Value,
                departmentLocations,
                departmentId);
        }

        if (department.IsFailure)
        {
            return department.Error.ToErrors();
        }

        await _departmentRepository.AddAsync(department.Value, cancellationToken);

        var saveChangeResult = await _departmentRepository.SaveChangesAsync(cancellationToken);
        if (saveChangeResult.IsFailure)
        {
            return saveChangeResult.Error;
        }

        _logger.LogInformation(
            "Department {DepartmentName} created with id {DepartmentId}",
            department.Value.Name.Value,
            department.Value.Id.Value);

        return department.Value.Id.Value;
    }
}