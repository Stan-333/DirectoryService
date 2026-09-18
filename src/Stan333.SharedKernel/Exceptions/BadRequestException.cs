using System.Text.Json;

namespace Stan333.SharedKernel.Exceptions;

public class BadRequestException : Exception
{
    protected BadRequestException(Error[] errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}