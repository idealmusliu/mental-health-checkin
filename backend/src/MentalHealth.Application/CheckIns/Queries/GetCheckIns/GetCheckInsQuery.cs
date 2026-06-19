namespace MentalHealth.Application.CheckIns.Queries.GetCheckIns;

public sealed record GetCheckInsQuery
{
    public Guid? UserId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
