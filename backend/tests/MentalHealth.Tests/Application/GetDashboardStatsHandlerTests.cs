using FluentAssertions;
using MentalHealth.Application.Dashboard.Queries.GetDashboardStats;
using MentalHealth.Domain.Entities;
using MentalHealth.Tests.Common;

namespace MentalHealth.Tests.Application;

public class GetDashboardStatsHandlerTests
{
    private static readonly DateTime Day1 = new(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day2 = new(2026, 6, 2, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handle_AggregatesTotalsAverageAndDailyTrend()
    {
        await using var db = TestData.CreateContext();
        db.Users.AddRange(TestData.Bob(), TestData.Carol());
        db.CheckIns.AddRange(
            CheckIn.Create(TestData.BobId, 2, null, Day1),
            CheckIn.Create(TestData.CarolId, 4, null, Day1), // Day1 avg = 3
            CheckIn.Create(TestData.BobId, 5, null, Day2));   // Day2 avg = 5
        await db.SaveChangesAsync();

        var handler = new GetDashboardStatsHandler(db);

        var result = await handler.Handle(new GetDashboardStatsQuery());

        result.IsSuccess.Should().BeTrue();
        var stats = result.Value!;
        stats.TotalCheckIns.Should().Be(3);
        stats.AverageMood.Should().BeApproximately(3.67, 0.01);

        stats.MoodOverTime.Should().HaveCount(2);
        stats.MoodOverTime[0].Date.Should().Be(Day1.Date);
        stats.MoodOverTime[0].AverageMood.Should().Be(3);
        stats.MoodOverTime[1].AverageMood.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WithNoData_ReturnsZeroes()
    {
        await using var db = TestData.CreateContext();
        var handler = new GetDashboardStatsHandler(db);

        var result = await handler.Handle(new GetDashboardStatsQuery());

        result.Value!.TotalCheckIns.Should().Be(0);
        result.Value.AverageMood.Should().Be(0);
        result.Value.MoodOverTime.Should().BeEmpty();
    }
}
