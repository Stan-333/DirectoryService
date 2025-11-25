using System.Text.Json;
using CSharpFunctionalExtensions;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Validation;

public static class CustomValidators
{
    // пример взят в документации на сайте https://docs.fluentvalidation.net/en/latest/custom-validators.html
    public static IRuleBuilderOptionsConditions<T, TElement> MustBeValueObject<T, TElement, TValueObject>(
        this IRuleBuilder<T, TElement> ruleBuilder,
        Func<TElement, Result<TValueObject, Error>> factoryMethod)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            Result<TValueObject, Error> result = factoryMethod(value);
            if (result.IsFailure)
            {
                context.AddFailure(JsonSerializer.Serialize<Error>(result.Error));
            }
        });
    }

    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule, Error error)
    {
        return rule.WithMessage(JsonSerializer.Serialize<Error>(error));
    }
}