using System.Text.Json;

namespace Shared.SharedKernel.Exceptions;

public class AuthenticationException : Exception
{
    protected AuthenticationException(Error[] errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}