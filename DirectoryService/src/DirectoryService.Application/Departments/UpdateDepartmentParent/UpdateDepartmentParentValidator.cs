using DirectoryService.Application.Validation;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Departments.UpdateDepartmentParent;

public class UpdateDepartmentParentValidator : AbstractValidator<UpdateDepartmentParentCommand>
{
    public UpdateDepartmentParentValidator()
    {
        RuleFor(c => c.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("DepartmentId"));

        RuleFor(c => c.Request)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("Request"));
    }
}