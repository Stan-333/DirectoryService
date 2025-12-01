using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(c => c.Request)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("Request"));

        RuleFor(c => c.Request.Name)
            .MustBeValueObject(DepartmentName.Create);

        RuleFor(c => c.Request.Identifier)
            .MustBeValueObject(Identifier.Create);

        RuleFor(c => c.Request.LocationIds)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("LocationIds"))
            .Must(ids => ids.Count > 0)
            .WithError(GeneralErrors.ValueIsInvalid("LocationIds"))
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithError(Error.Validation("value.is.invalid", "Список LocationIds содержит пустые id"))
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithError(GeneralErrors.ListHasDuplicates("LocationIds"));

        RuleFor(c => c.Request.ParentId)
            .NotEmpty()
            .When(c => c.Request.ParentId != null)
            .WithError(GeneralErrors.ValueIsInvalid("ParentId"));
    }
}