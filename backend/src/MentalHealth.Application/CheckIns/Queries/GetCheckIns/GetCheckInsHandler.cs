using FluentValidation;
using MentalHealth.Application.CheckIns.Dtos;
using MentalHealth.Application.Common;
using MentalHealth.Application.Common.Abstractions;
using MentalHealth.Application.Common.Extensions;
using MentalHealth.Application.Common.Interfaces;
using MentalHealth.Application.Common.Models;
using MentalHealth.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Application.CheckIns.Queries.GetCheckIns;

public sealed class GetCheckInsHandler : IQueryHandler<GetCheckInsQuery, Result<PagedResult<CheckInDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetCheckInsQuery> _validator;

    public GetCheckInsHandler(IApplicationDbContext db, IValidator<GetCheckInsQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<Result<PagedResult<CheckInDto>>> Handle(GetCheckInsQuery query, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            return Result<PagedResult<CheckInDto>>.Validation(validation.ToErrorDictionary());

        // timestamptz comparisons require UTC-kind DateTimes. `to` is an inclusive
        // calendar day, so we compare against the start of the following day.
        var fromUtc = UtcRange.Start(query.From);
        var toUtc = UtcRange.End(query.To);

        var filtered = _db.CheckIns
            .AsNoTracking()
            .Where(c => query.UserId == null || c.UserId == query.UserId)
            .Where(c => fromUtc == null || c.CreatedAt >= fromUtc)
            .Where(c => toUtc == null || c.CreatedAt < toUtc);

        var totalCount = await filtered.CountAsync(cancellationToken);

        var items = await filtered
            .OrderByDescending(c => c.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(CheckInDto.Projection)
            .ToListAsync(cancellationToken);

        var page = new PagedResult<CheckInDto>(items, query.Page, query.PageSize, totalCount);
        return Result<PagedResult<CheckInDto>>.Success(page);
    }
}
