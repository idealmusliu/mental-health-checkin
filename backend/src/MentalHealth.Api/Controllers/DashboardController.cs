using MentalHealth.Api.Common;
using MentalHealth.Application.Common.Abstractions;
using MentalHealth.Application.Common.Interfaces;
using MentalHealth.Application.Common.Results;
using MentalHealth.Application.Dashboard.Dtos;
using MentalHealth.Application.Dashboard.Queries.GetDashboardStats;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealth.Api.Controllers;

[ApiController]
[Route("dashboard")]
[Produces("application/json")]
public sealed class DashboardController : ControllerBase
{
    private readonly IQueryHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>> _stats;
    private readonly ICurrentUser _currentUser;

    public DashboardController(
        IQueryHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>> stats,
        ICurrentUser currentUser)
    {
        _stats = stats;
        _currentUser = currentUser;
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetStats(
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int tzOffsetMinutes = 0,
        CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Problem(detail: "Authentication is required.", statusCode: StatusCodes.Status401Unauthorized);

        if (!_currentUser.IsManager)
            return Problem(detail: "Only managers can view the dashboard.", statusCode: StatusCodes.Status403Forbidden);

        var query = new GetDashboardStatsQuery
        {
            UserId = userId,
            From = from,
            To = to,
            TimezoneOffsetMinutes = tzOffsetMinutes
        };
        var result = await _stats.Handle(query, ct);
        return result.ToOk(this);
    }
}
