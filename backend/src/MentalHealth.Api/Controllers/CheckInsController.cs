using MentalHealth.Api.Common;
using MentalHealth.Api.Contracts;
using MentalHealth.Application.CheckIns.Commands.CreateCheckIn;
using MentalHealth.Application.CheckIns.Commands.UpdateCheckIn;
using MentalHealth.Application.CheckIns.Dtos;
using MentalHealth.Application.CheckIns.Queries.GetCheckInById;
using MentalHealth.Application.CheckIns.Queries.GetCheckIns;
using MentalHealth.Application.Common.Abstractions;
using MentalHealth.Application.Common.Interfaces;
using MentalHealth.Application.Common.Models;
using MentalHealth.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealth.Api.Controllers;

[ApiController]
[Route("checkins")]
[Produces("application/json")]
public sealed class CheckInsController : ControllerBase
{
    private readonly ICommandHandler<CreateCheckInCommand, Result<CheckInDto>> _create;
    private readonly ICommandHandler<UpdateCheckInCommand, Result<CheckInDto>> _update;
    private readonly IQueryHandler<GetCheckInsQuery, Result<PagedResult<CheckInDto>>> _list;
    private readonly IQueryHandler<GetCheckInByIdQuery, Result<CheckInDto>> _getById;
    private readonly ICurrentUser _currentUser;

    public CheckInsController(
        ICommandHandler<CreateCheckInCommand, Result<CheckInDto>> create,
        ICommandHandler<UpdateCheckInCommand, Result<CheckInDto>> update,
        IQueryHandler<GetCheckInsQuery, Result<PagedResult<CheckInDto>>> list,
        IQueryHandler<GetCheckInByIdQuery, Result<CheckInDto>> getById,
        ICurrentUser currentUser)
    {
        _create = create;
        _update = update;
        _list = list;
        _getById = getById;
        _currentUser = currentUser;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CheckInDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Create([FromBody] CreateCheckInRequest request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return Problem(detail: "Authentication is required.", statusCode: StatusCodes.Status401Unauthorized);

        if (_currentUser.IsManager)
            return Problem(
                detail: "Managers review check-ins; only employees can submit them.",
                statusCode: StatusCodes.Status403Forbidden);

        // The author is always the authenticated caller — the body cannot set it.
        var command = new CreateCheckInCommand(_currentUser.UserId!.Value, request.Mood, request.Notes);
        var result = await _create.Handle(command, ct);

        if (result.IsFailure)
            return result.ToProblem(this);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CheckInDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> List(
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Problem(detail: "Authentication is required.", statusCode: StatusCodes.Status401Unauthorized);

        var query = new GetCheckInsQuery
        {
            UserId = EmployeeScope() ?? userId,
            From = from,
            To = to,
            Page = page,
            PageSize = pageSize
        };

        var result = await _list.Handle(query, ct);
        return result.ToOk(this);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CheckInDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetById(Guid id, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return Problem(detail: "Authentication is required.", statusCode: StatusCodes.Status401Unauthorized);

        var result = await _getById.Handle(new GetCheckInByIdQuery(id, EmployeeScope()), ct);
        return result.ToOk(this);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CheckInDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateCheckInRequest request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return Problem(detail: "Authentication is required.", statusCode: StatusCodes.Status401Unauthorized);

        if (_currentUser.IsManager)
            return Problem(
                detail: "Managers review check-ins; they cannot modify them.",
                statusCode: StatusCodes.Status403Forbidden);

        // Employees may only edit their own check-in.
        var command = new UpdateCheckInCommand(id, request.Mood, request.Notes, _currentUser.UserId!.Value);
        var result = await _update.Handle(command, ct);
        return result.ToOk(this);
    }

    // Reads require authentication (guarded above); employees are limited to their
    // own check-ins, while managers are unrestricted.
    private Guid? EmployeeScope() =>
        _currentUser is { IsAuthenticated: true, IsManager: false } ? _currentUser.UserId : null;
}
