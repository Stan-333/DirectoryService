using System.Text.Json;

namespace Stan333.SharedKernel.Exceptions;

public class FailureException : Exception
{
    protected FailureException(Error[] errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}