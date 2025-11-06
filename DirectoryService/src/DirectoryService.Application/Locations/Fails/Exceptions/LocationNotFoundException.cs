using DirectoryService.Application.Exceptions;
using Shared;

namespace DirectoryService.Application.Locations.Fails.Exceptions;

public class LocationNotFoundException : NotFoundException
{
    public LocationNotFoundException(Error[] errors)
        : base(errors)
    {
    }
}