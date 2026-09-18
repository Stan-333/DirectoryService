using System.Text.Json;

namespace Shared.SharedKernel.Exceptions;

public class ValidationException : Exception
{
    protected ValidationException(Error[] errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}