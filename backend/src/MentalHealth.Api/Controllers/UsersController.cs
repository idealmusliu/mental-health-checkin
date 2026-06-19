using MentalHealth.Application.Common.Abstractions;
using MentalHealth.Application.Users.Dtos;
using MentalHealth.Application.Users.Queries.GetUsers;
using Microsoft.AspNetCore.Mvc;

namespace MentalHealth.Api.Controllers;

[ApiController]
[Route("users")]
[Produces("application/json")]
public sealed class UsersController : ControllerBase
{
    private readonly IQueryHandler<GetUsersQuery, IReadOnlyList<UserDto>> _getUsers;

    public UsersController(IQueryHandler<GetUsersQuery, IReadOnlyList<UserDto>> getUsers) => _getUsers = getUsers;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll(CancellationToken ct)
    {
        var users = await _getUsers.Handle(new GetUsersQuery(), ct);
        return Ok(users);
    }
}
