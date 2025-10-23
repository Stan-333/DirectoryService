using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using TimeZone = DirectoryService.Domain.Locations.TimeZone;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateLocationRequest> _validator;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(
        ILocationsRepository locationsRepository,
        IValidator<CreateLocationRequest> validator,
        ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateLocationRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var location = Location.Create(
            LocationName.Create(request.Name).Value,
            Address.Create(
                request.Address.PostalCode,
                request.Address.Region,
                request.Address.City,
                request.Address.Street,
                request.Address.House,
                request.Address.Apartment).Value,
            TimeZone.Create(request.Timezone).Value,
            true,
            DateTime.Now,
            null);

        await _locationsRepository.AddAsync(location.Value, cancellationToken);

        _logger.LogInformation("Location created with id {LocationId}", location.Value.Id.Value);

        return location.Value.Id.Value;
    }
}