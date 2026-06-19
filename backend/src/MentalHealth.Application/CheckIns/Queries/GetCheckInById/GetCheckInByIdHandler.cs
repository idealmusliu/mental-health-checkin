using MentalHealth.Application.CheckIns.Dtos;
using MentalHealth.Application.Common.Abstractions;
using MentalHealth.Application.Common.Interfaces;
using MentalHealth.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Application.CheckIns.Queries.GetCheckInById;

public sealed class GetCheckInByIdHandler : IQueryHandler<GetCheckInByIdQuery, Result<CheckInDto>>
{
    private readonly IApplicationDbContext _db;

    public GetCheckInByIdHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<CheckInDto>> Handle(GetCheckInByIdQuery query, CancellationToken cancellationToken = default)
    {
        var dto = await _db.CheckIns
            .AsNoTracking()
            .Where(c => c.Id == query.Id)
            .Where(c => query.RestrictToUserId == null || c.UserId == query.RestrictToUserId)
            .Select(CheckInDto.Projection)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result<CheckInDto>.NotFound($"Check-in '{query.Id}' was not found.")
            : Result<CheckInDto>.Success(dto);
    }
}
