using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionHandler : ICommandHandler<Guid, CreatePositionCommand>
{
    private readonly IPositionRepository _positionRepository;
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly ILogger<CreatePositionHandler> _logger;

    public CreatePositionHandler(
        IPositionRepository positionRepository,
        IValidator<CreatePositionCommand> validator,
        ILogger<CreatePositionHandler> logger)
    {
        _positionRepository = positionRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreatePositionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        PositionName positionName = PositionName.Create(command.Request.Name).Value;
        if (await _positionRepository.IsActiveDepartmentNameExistAsync(positionName, cancellationToken))
        {
            return GeneralErrors.AlreadyExist("position").ToErrors();
        }

        foreach (Guid guid in command.Request.DepartmentIds)
        {
            if (!await _positionRepository.IsActiveDepartmentExistAsync(
                    new DepartmentId(guid),
                    cancellationToken))
            {
                return Error.NotFound(
                    "department.not.found",
                    $"Подразделение с id - {guid} отсутствует",
                    guid).ToErrors();
            }
        }

        PositionId positionId = new PositionId(Guid.NewGuid());
        var departmentPositions = command.Request.DepartmentIds
            .Select(depId => new DepartmentPosition(new DepartmentId(depId), positionId)).ToList();

        var position = Position.Create(
            positionName,
            command.Request.Description,
            departmentPositions,
            positionId);

        await _positionRepository.AddAsync(position.Value, cancellationToken);

        var saveChangeResult = await _positionRepository.SaveChangesAsync(cancellationToken);
        if (saveChangeResult.IsFailure)
        {
            return saveChangeResult.Error;
        }

        _logger.LogInformation(
            "Position {PositionName} created with id {PositionId}",
            position.Value.Name.Value,
            position.Value.Id.Value);

        return position.Value.Id.Value;
    }
}