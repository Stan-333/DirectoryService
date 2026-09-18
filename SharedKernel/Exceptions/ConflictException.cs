using System.Text.Json;

namespace Shared.SharedKernel.Exceptions;

public class ConflictException : Exception
{
    protected ConflictException(Error[] errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}