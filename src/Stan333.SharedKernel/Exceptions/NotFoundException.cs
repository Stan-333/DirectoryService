using System.Text.Json;

namespace Stan333.SharedKernel.Exceptions;

public class NotFoundException : Exception
{
    protected NotFoundException(Error[] errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}