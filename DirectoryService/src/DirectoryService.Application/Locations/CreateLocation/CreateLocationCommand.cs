using DirectoryService.Contracts.Locations.Requests;
using Shared.Core.Abstractions;

namespace DirectoryService.Application.Locations.CreateLocation;

public record CreateLocationCommand(CreateLocationRequest Request) : ICommand;