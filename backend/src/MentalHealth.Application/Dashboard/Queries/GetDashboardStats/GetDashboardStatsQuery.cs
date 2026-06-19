namespace MentalHealth.Application.Dashboard.Queries.GetDashboardStats;

public sealed record GetDashboardStatsQuery
{
    public Guid? UserId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }

    // Minutes to add to UTC for the caller's local day (UTC+2 => 120).
    public int TimezoneOffsetMinutes { get; init; }
}
