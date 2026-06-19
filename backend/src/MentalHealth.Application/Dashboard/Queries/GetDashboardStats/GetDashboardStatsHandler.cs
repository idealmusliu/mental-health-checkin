using MentalHealth.Application.Common;
using MentalHealth.Application.Common.Abstractions;
using MentalHealth.Application.Common.Interfaces;
using MentalHealth.Application.Common.Results;
using MentalHealth.Application.Dashboard.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth.Application.Dashboard.Queries.GetDashboardStats;

public sealed class GetDashboardStatsHandler : IQueryHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IApplicationDbContext _db;

    public GetDashboardStatsHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery query, CancellationToken cancellationToken = default)
    {
        // `to` is an inclusive calendar day, compared against the start of the next day.
        var fromUtc = UtcRange.Start(query.From);
        var toUtc = UtcRange.End(query.To);

        // Bucket in memory by the caller's local day so chart dates match the UI.
        var rows = await _db.CheckIns
            .AsNoTracking()
            .Where(c => query.UserId == null || c.UserId == query.UserId)
            .Where(c => fromUtc == null || c.CreatedAt >= fromUtc)
            .Where(c => toUtc == null || c.CreatedAt < toUtc)
            .Select(c => new { c.CreatedAt, c.Mood })
            .ToListAsync(cancellationToken);

        var offset = TimeSpan.FromMinutes(query.TimezoneOffsetMinutes);

        var moodOverTime = rows
            .GroupBy(r => DateTime.SpecifyKind(r.CreatedAt, DateTimeKind.Utc).Add(offset).Date)
            .Select(g => new MoodTrendPointDto
            {
                Date = g.Key,
                AverageMood = Math.Round(g.Average(x => (double)x.Mood), 2),
                Count = g.Count()
            })
            .OrderBy(p => p.Date)
            .ToList();

        var stats = new DashboardStatsDto
        {
            TotalCheckIns = rows.Count,
            AverageMood = rows.Count == 0 ? 0d : Math.Round(rows.Average(r => (double)r.Mood), 2),
            MoodOverTime = moodOverTime
        };

        return Result<DashboardStatsDto>.Success(stats);
    }
}
