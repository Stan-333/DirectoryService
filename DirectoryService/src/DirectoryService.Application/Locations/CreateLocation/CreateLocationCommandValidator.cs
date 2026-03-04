using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Shared;
using TimeZone = DirectoryService.Domain.Locations.TimeZone;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("Request"));

        RuleFor(c => c.Request.Name)
            .MustBeValueObject(LocationName.Create);

        RuleFor(a => a.Request.Address)
            .MustBeValueObject(c => Address.Create(
                c.PostalCode,
                c.Region,
                c.City,
                c.Street,
                c.House,
                c.Apartment));

        RuleFor(x => x.Request.Timezone)
            .MustBeValueObject(TimeZone.Create);
    }
}