using FluentAssertions;
using Stan333.Framework.EndpointResults;
using Stan333.SharedKernel;

namespace Stan333.Shared.UnitTests.Framework;

public class ErrorStatusCodesTests
{
    [Theory]
    [InlineData(ErrorType.Validation, 400)]
    [InlineData(ErrorType.Authentication, 401)]
    [InlineData(ErrorType.Authorization, 403)]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.Conflict, 409)]
    [InlineData(ErrorType.Failure, 500)]
    public void FromErrorType_maps_each_type_to_status(ErrorType errorType, int expectedStatus)
    {
        ErrorStatusCodes.FromErrorType(errorType).Should().Be(expectedStatus);
    }

    [Fact]
    public void FromErrors_with_errors_of_one_type_returns_status_of_that_type()
    {
        Error[] errors = [Error.Validation(null, "a"), Error.Validation(null, "b")];

        ErrorStatusCodes.FromErrors(errors).Should().Be(400);
    }

    [Fact]
    public void FromErrors_with_different_client_errors_returns_status_of_first_error()
    {
        Error[] validationFirst = [Error.Validation(null, "a"), Error.NotFound(null, "b")];
        Error[] notFoundFirst = [Error.NotFound(null, "b"), Error.Validation(null, "a")];

        ErrorStatusCodes.FromErrors(validationFirst).Should().Be(400);
        ErrorStatusCodes.FromErrors(notFoundFirst).Should().Be(404);
    }

    [Fact]
    public void FromErrors_with_failure_among_errors_returns_500()
    {
        Error[] errors = [Error.Validation(null, "a"), Error.Failure(null, "b")];

        ErrorStatusCodes.FromErrors(errors).Should().Be(500);
    }

    [Fact]
    public void FromErrors_with_no_errors_returns_500()
    {
        ErrorStatusCodes.FromErrors([]).Should().Be(500);
    }
}