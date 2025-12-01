using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator()
    {
        RuleFor(c => c.Request)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("Request"));
        
        RuleFor(c => c.Request.Name)
            .MustBeValueObject(PositionName.Create);
        
        RuleFor(c => c.Request.DepartmentIds)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("DepartmentIds"))
            .Must(ids => ids.Count > 0)
            .WithError(GeneralErrors.ValueIsInvalid("DepartmentIds"))
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithError(Error.Validation("value.is.invalid", "Список DepartmentIds содержит пустые id"))
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithError(GeneralErrors.ListHasDuplicates("DepartmentIds"));
        
        RuleFor(c => c.Request.Description)
            .MaximumLength(LengthConstants.LENGTH1000)
            .WithError(GeneralErrors.ValueIsInvalid("Description"));
    }
}