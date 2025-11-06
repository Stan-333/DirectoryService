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
            .NotEmpty().WithMessage("Имя не может быть пустым")
            .NotNull().WithMessage("Имя не может быть Null")
            .MinimumLength(LengthConstants.LENGTH3).WithMessage("Имя не должно быть меньше трёх символов")
            .MaximumLength(LengthConstants.LENGTH120).WithMessage("Имя не должно быть больше 120 символов");

        RuleFor(x => x.Address.PostalCode)
            .Must(x => Regex.IsMatch(x, @"^[0-9]{6}$"))
            .WithMessage("Почтовый индекс должен состоять из шести цифр");

        RuleFor(x => x.Address.Region)
            .NotEmpty().WithMessage("Регион не может быть пустым")
            .NotNull().WithMessage("Регион не может быть Null")
            .MaximumLength(LengthConstants.LENGTH100).WithMessage("Регион не должен быть больше 100 символов");

        RuleFor(x => x.Address.City)
            .NotEmpty().WithMessage("Название города не может быть пустым")
            .NotNull().WithMessage("Название города не может быть Null")
            .MaximumLength(LengthConstants.LENGTH100).WithMessage("Название города не должно быть больше 100 символов");

        RuleFor(x => x.Address.Street)
            .NotEmpty().WithMessage("Название улицы не должно быть пустым")
            .NotNull().WithMessage("Название улицы не может быть Null")
            .MaximumLength(LengthConstants.LENGTH100).WithMessage("Название улицы не должно быть больше 100 символов");

        RuleFor(x => x.Address.House)
            .NotEmpty().WithMessage("Номер дома не может быть пустым")
            .NotNull().WithMessage("Номер дома не может быть Null")
            .MaximumLength(LengthConstants.LENGTH10).WithMessage("Номер дома не должен быть больше 10 символов");

        RuleFor(x => x.Address.Apartment)
            .MaximumLength(LengthConstants.LENGTH10);

        RuleFor(x => x.Timezone)
            .NotEmpty().WithMessage("Timezone не может быть пустым")
            .NotNull().WithMessage("Timezone не может быть Null")
            .Must(x => TZConvert.KnownIanaTimeZoneNames.Contains(x)).WithMessage("Значение не соответствует формату IANA");
    }
}