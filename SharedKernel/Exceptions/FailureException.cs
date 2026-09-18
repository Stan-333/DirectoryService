using System.Text.Json;

namespace Shared.SharedKernel.Exceptions;

public class FailureException : Exception
{
    protected FailureException(Error[] errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}