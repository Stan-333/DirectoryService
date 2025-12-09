using DirectoryService.Application.Validation;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Departments.UpdateDepartmentLocations;

public class UpdateDepartmentLocationsValidator : AbstractValidator<UpdateDepartmentLocationsCommand>
{
    public UpdateDepartmentLocationsValidator()
    {
        RuleFor(c => c.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("DepartmentId"));

        RuleFor(c => c.Request)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("Request"));

        RuleFor(c => c.Request.LocationIds)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("LocationIds"))
            .Must(ids => ids.Count > 0)
            .WithError(GeneralErrors.ValueIsInvalid("LocationIds"))
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithError(Error.Validation("value.is.invalid", "Список LocationIds содержит пустые id"))
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithError(GeneralErrors.ListHasDuplicates("LocationIds"));
    }
}