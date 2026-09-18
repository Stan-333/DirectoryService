using System.Text.Json;

namespace Stan333.SharedKernel.Exceptions;

public class AuthenticationException : Exception
{
    protected AuthenticationException(Error[] errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}