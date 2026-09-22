using FluentAssertions;
using Shared.Kernel.Exceptions;

namespace Shared.Kernel.Tests;

public class AppExceptionTests
{
    [Fact]
    public void Message_contains_codes_and_messages_of_all_errors()
    {
        Errors errors = new[] { Error.NotFound("a.not.found", "A не найден"), Error.Conflict("b.conflict", "B занят") };

        var exception = new AppException(errors);

        exception.Message.Should().Be("a.not.found: A не найден; b.conflict: B занят");
        exception.Errors.Should().Equal(errors);
    }

    [Fact]
    public void Single_error_constructor_wraps_error_into_errors()
    {
        Error error = Error.Authorization(null, "Нет доступа");

        var exception = new AppException(error);

        exception.Errors.Should().ContainSingle().Which.Should().Be(error);
    }

    [Fact]
    public void Empty_errors_are_rejected()
    {
        Action create = () => _ = new AppException(new Errors([]));

        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Null_error_is_rejected()
    {
        Action create = () => _ = new AppException((Error)null!);

        create.Should().Throw<ArgumentNullException>();
    }
}