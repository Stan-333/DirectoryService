using Shared;

namespace DirectoryService.Application.Locations.Fails;

public partial class Errors
{
    public static class Locations
    {
        public static Error LocationNotFound(Guid id) => Error.NotFound("location.not.found", "Location not found", id);

        public static Error LocationValidation(string? code, string message) => Error.Validation(code, message);
    }
}