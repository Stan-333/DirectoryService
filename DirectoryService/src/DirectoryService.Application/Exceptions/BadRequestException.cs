using System.Text.Json;
using Shared;

namespace DirectoryService.Application.Exceptions;

public class BadRequestException : Exception
{
    protected BadRequestException(Error[] errors)
        : base(JsonSerializer.Serialize(errors))
    {
    }
}