using FluentAssertions;
using Shared.Kernel;

namespace Shared.UnitTests.Kernel;

public class GeneralErrorsTests
{
    [Fact]
    public void ListHasDuplicates_has_its_own_code()
    {
        Error error = GeneralErrors.ListHasDuplicates("LocationIds");

        error.Code.Should().Be("list.has.duplicates");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void NotFound_puts_id_into_message()
    {
        Guid id = Guid.NewGuid();

        Error error = GeneralErrors.NotFound(id, "Подразделение");

        error.Type.Should().Be(ErrorType.NotFound);
        error.Message.Should().Contain(id.ToString());
    }
}