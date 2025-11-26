using System.Text.Json;
using FluentValidation.Results;
using Shared;

namespace DirectoryService.Application.Validation;

public static class ValidationExtensions
{
    // public static Errors ToErrors(this ValidationResult validationResult) =>
    //     validationResult.Errors
    //         .Select(e => Error.Validation(e.ErrorCode, e.ErrorMessage, e.PropertyName)).ToList();

    // Добавление метода ToErrors() к классу ValidationResult
    public static Errors ToErrors(this ValidationResult validationResult)
    {
        var validationErrors = validationResult.Errors;

        var errors = from validationError in validationErrors
            let error = JsonSerializer.Deserialize<Error>(validationError.ErrorMessage)
            select Error.Validation(error.Code, error.Message, validationError.PropertyName);

        return errors.ToList();
    }
}