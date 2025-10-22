using System.Text.RegularExpressions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using TimeZoneConverter;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationValidator : AbstractValidator<CreateLocationRequest>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .NotNull()
            .MinimumLength(LengthConstants.LENGTH3)
            .MaximumLength(LengthConstants.LENGTH120);

        RuleFor(x => x.Address.PostalCode)
            .Must(x => Regex.IsMatch(x, @"^[0-9]{6}$"))
            .WithMessage("Почтовый индекс должен состоять из шести цифр");

        RuleFor(x => x.Address.Region)
            .NotEmpty()
            .NotNull()
            .MaximumLength(LengthConstants.LENGTH100);

        RuleFor(x => x.Address.City)
            .NotEmpty()
            .NotNull()
            .MaximumLength(LengthConstants.LENGTH100);

        RuleFor(x => x.Address.Street)
            .NotEmpty()
            .NotNull()
            .MaximumLength(LengthConstants.LENGTH100);

        RuleFor(x => x.Address.House)
            .NotEmpty()
            .NotNull()
            .MaximumLength(LengthConstants.LENGTH10);

        RuleFor(x => x.Address.Apartment)
            .MaximumLength(LengthConstants.LENGTH10);

        RuleFor(x => x.Timezone)
            .NotEmpty()
            .NotNull()
            .Must(x => TZConvert.KnownIanaTimeZoneNames.Contains(x))
            .WithMessage("Значение не соответствует формату IANA");
    }
}