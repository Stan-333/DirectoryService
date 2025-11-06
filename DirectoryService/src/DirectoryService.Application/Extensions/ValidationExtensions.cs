using FluentValidation.Results;
using Shared;

namespace DirectoryService.Application.Extensions;

public static class ValidationExtensions
{
    // Добавление метода к классу ValidationResult
    public static Errors ToErrors(this ValidationResult validationResult) =>
        validationResult.Errors
            .Select(e => Error.Validation(e.ErrorCode, e.ErrorMessage, e.PropertyName)).ToList();
}