using MentalHealth.Application.Common.Abstractions;
using MentalHealth.Application.Common.Interfaces;
using MentalHealth.Application.Users.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Application.Users.Queries.GetUsers;

public sealed class GetUsersHandler : IQueryHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IApplicationDbContext _db;

    public GetUsersHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        // enum.ToString() isn't reliably translatable, so map after materializing.
        var users = await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);

        return users.Select(UserDto.FromEntity).ToList();
    }
}
