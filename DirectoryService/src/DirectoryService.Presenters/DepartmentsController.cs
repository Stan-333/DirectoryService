using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.Queries;
using DirectoryService.Application.Departments.UpdateDepartmentLocations;
using DirectoryService.Application.Departments.UpdateDepartmentParent;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Contracts.Departments.Responses;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.EndpointResults;

namespace DirectoryService.Presenters;

[ApiController]
[Route("api/departments")]
public class DepartmentsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromBody] CreateDepartmentRequest request,
        [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand(request);

        Result<Guid, Errors> result = await handler.Handle(command, cancellationToken);

        return result;
    }

    [HttpPut("/api/departments/{departmentId:guid}/locations")]
    public async Task<EndpointResult<Guid>> UpdateLocations(
        [FromRoute] Guid departmentId,
        [FromBody] UpdateDepartmentLocationsRequest request,
        [FromServices] ICommandHandler<Guid, UpdateDepartmentLocationsCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentLocationsCommand(departmentId, request);

        var result = await handler.Handle(command, cancellationToken);

        return result;
    }

    [HttpPut("/api/departments/{departmentId:guid}/parent")]
    public async Task<EndpointResult<Guid>> UpdateParent(
        [FromRoute] Guid departmentId,
        [FromBody] UpdateDepartmentParentRequest request,
        [FromServices] ICommandHandler<Guid, UpdateDepartmentParentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentParentCommand(departmentId, request);

        var result = await handler.Handle(command, cancellationToken);

        return result;
    }

    [HttpGet("/api/departments/top-positions")]
    public async Task<Envelope<GetTopDepartmentsByPositionResponse>> GetTopFiveByPositions(
        [FromQuery] GetTopDepartmentsByPositionsRequest request,
        [FromServices] IQueryHandler<GetTopDepartmentsByPositionResponse, GetTopDepartmentsByPositionQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetTopDepartmentsByPositionQuery(request);

        var result = await handler.Handle(query, cancellationToken);

        return Envelope<GetTopDepartmentsByPositionResponse>.Ok(result);
    }
}