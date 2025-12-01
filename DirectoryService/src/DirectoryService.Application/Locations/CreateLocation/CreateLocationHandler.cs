using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;
using TimeZone = DirectoryService.Domain.Locations.TimeZone;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateLocationCommand> _validator;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(
        ILocationsRepository locationsRepository,
        IValidator<CreateLocationCommand> validator,
        ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        LocationName locationName = LocationName.Create(command.Request.Name).Value;
        if (await _locationsRepository.IsActiveLocationNameExistAsync(locationName, cancellationToken))
        {
            return Error.Validation("record.already.exist", "Локация с таким именем уже существует")
                .ToErrors();
        }

        Address address = Address.Create(
            command.Request.Address.PostalCode,
            command.Request.Address.Region,
            command.Request.Address.City,
            command.Request.Address.Street,
            command.Request.Address.House,
            command.Request.Address.Apartment).Value;
        if (await _locationsRepository.IsActiveAddressExistAsync(address, cancellationToken))
        {
            return Error.Validation("record.already.exist", "Локация с таким адресом уже существует")
                .ToErrors();
        }

        var location = Location.Create(
            locationName,
            address,
            TimeZone.Create(command.Request.Timezone).Value,
            true,
            DateTime.Now);

        await _locationsRepository.AddAsync(location.Value, cancellationToken);

        var saveChangeResult = await _locationsRepository.SaveChangesAsync(cancellationToken);
        if (saveChangeResult.IsFailure)
        {
            return saveChangeResult.Error;
        }

        _logger.LogInformation(
            "Location {LocationName} created with id {LocationId}",
            location.Value.Name.Value,
            location.Value.Id.Value);

        return location.Value.Id.Value;
    }
}