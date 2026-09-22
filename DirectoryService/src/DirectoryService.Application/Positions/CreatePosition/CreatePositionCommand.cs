using DirectoryService.Contracts.Positions;
using Shared.Core.Abstractions;

namespace DirectoryService.Application.Positions.CreatePosition;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;