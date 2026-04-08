using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.Queries;
using DirectoryService.Application.Departments.SoftDeleteDepartment;
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

    [HttpGet("/api/departments/roots")]
    public async Task<Envelope<GetRootDepartmentsWithChildrenResponse>> GetRootDepartmentsWithChildren(
        [FromQuery] GetRootDepartmentsWithChildrenRequest request,
        [FromServices] IQueryHandler<GetRootDepartmentsWithChildrenResponse, GetRootDepartmentsWithChildrenQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetRootDepartmentsWithChildrenQuery(request);

        var result = await handler.Handle(query, cancellationToken);

        return Envelope<GetRootDepartmentsWithChildrenResponse>.Ok(result);
    }

    [HttpGet("/api/departments/{parentId:guid}/children")]
    public async Task<Envelope<GetChildrenDepartmentsResponse>> GetChildren(
        [FromRoute] Guid parentId,
        [FromQuery] GetChildrenDepartmentsRequest request,
        [FromServices] IQueryHandler<GetChildrenDepartmentsResponse, GetChildrenDepartmentsQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetChildrenDepartmentsQuery(parentId, request);

        var result = await handler.Handle(query, cancellationToken);

        return Envelope<GetChildrenDepartmentsResponse>.Ok(result);
    }

    [HttpDelete("/api/departments/{departmentId:guid}")]
    public async Task<EndpointResult<Guid>> SoftDelete(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<Guid, SoftDeleteDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new SoftDeleteDepartmentCommand(departmentId);

        var result = await handler.Handle(command, cancellationToken);

        return result;
    }
}