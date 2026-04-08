using DirectoryService.Application.Validation;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Departments.SoftDeleteDepartment;

public class SoftDeleteDepartmentValidator : AbstractValidator<SoftDeleteDepartmentCommand>
{
    public SoftDeleteDepartmentValidator()
    {
        RuleFor(c => c.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("DepartmentId"));
    }
}