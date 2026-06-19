namespace MentalHealth.Application.Dashboard.Dtos;

public sealed record DashboardStatsDto
{
    public int TotalCheckIns { get; init; }
    public double AverageMood { get; init; }
    public IReadOnlyList<MoodTrendPointDto> MoodOverTime { get; init; } = Array.Empty<MoodTrendPointDto>();
}

public sealed record MoodTrendPointDto
{
    public DateTime Date { get; init; }
    public double AverageMood { get; init; }
    public int Count { get; init; }
}
