using System.Text.Json;

namespace Stan333.SharedKernel.Exceptions;

public class ConflictException : Exception
{
    protected ConflictException(Error[] errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}