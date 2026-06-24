using FluentValidation.Results;
using Shared;

namespace DirectoryService.Application.Validation;

public static class ValidationExtensions
{
    // Добавление метода ToErrors() к классу ValidationResult
    public static Errors ToErrors(this ValidationResult validationResult)
    {
        return validationResult.Errors
            .Select(ve => ve.CustomState is Error e
                ? Error.Validation(e.Code, e.Message, ve.PropertyName)
                : Error.Validation(ve.ErrorCode, ve.ErrorMessage, ve.PropertyName))
            .ToList();
    }
}