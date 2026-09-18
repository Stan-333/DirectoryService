using System.Text.Json;

namespace Stan333.SharedKernel.Exceptions;

public class ValidationException : Exception
{
    protected ValidationException(Error[] errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}