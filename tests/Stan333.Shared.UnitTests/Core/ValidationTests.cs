using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentValidation;
using Stan333.Core.Validation;
using Stan333.SharedKernel;

namespace Stan333.Shared.UnitTests.Core;

public class ValidationTests
{
    [Fact]
    public void MustBeValueObject_turns_factory_error_into_validation_error_with_field()
    {
        var validator = new InlineValidator<Person>();
        validator.RuleFor(p => p.Name).MustBeValueObject(CreateName);

        Errors errors = validator.Validate(new Person(string.Empty)).ToErrors();

        Error error = errors.Should().ContainSingle().Subject;
        error.Code.Should().Be("value.is.required");
        error.Type.Should().Be(ErrorType.Validation);
        error.InvalidField.Should().Be(nameof(Person.Name));
    }

    [Fact]
    public void WithError_keeps_code_and_message_of_given_error()
    {
        var validator = new InlineValidator<Person>();
        validator.RuleFor(p => p.Name).NotEmpty().WithError(Error.Validation("name.empty", "Имя пустое"));

        Errors errors = validator.Validate(new Person(string.Empty)).ToErrors();

        Error error = errors.Should().ContainSingle().Subject;
        error.Code.Should().Be("name.empty");
        error.Message.Should().Be("Имя пустое");
        error.InvalidField.Should().Be(nameof(Person.Name));
    }

    [Fact]
    public void Ordinary_rule_failure_becomes_validation_error_with_its_code()
    {
        var validator = new InlineValidator<Person>();
        validator.RuleFor(p => p.Name).MaximumLength(3);

        Errors errors = validator.Validate(new Person("слишком длинное")).ToErrors();

        Error error = errors.Should().ContainSingle().Subject;
        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be("MaximumLengthValidator");
        error.InvalidField.Should().Be(nameof(Person.Name));
    }

    private static Result<string, Error> CreateName(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? GeneralErrors.ValueIsRequired("name")
            : value;

    public sealed record Person(string Name);
}