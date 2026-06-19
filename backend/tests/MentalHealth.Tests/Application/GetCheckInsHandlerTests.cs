using FluentAssertions;
using MentalHealth.Application.CheckIns.Queries.GetCheckIns;
using MentalHealth.Domain.Entities;
using MentalHealth.Infrastructure.Persistence;
using MentalHealth.Tests.Common;

namespace MentalHealth.Tests.Application;

public class GetCheckInsHandlerTests
{
    private static readonly DateTime Day1 = new(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day5 = new(2026, 6, 5, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day10 = new(2026, 6, 10, 9, 0, 0, DateTimeKind.Utc);

    private static async Task<ApplicationDbContext> SeededContextAsync()
    {
        var db = TestData.CreateContext();
        db.Users.AddRange(TestData.Bob(), TestData.Carol());
        db.CheckIns.AddRange(
            CheckIn.Create(TestData.BobId, 3, "bob day1", Day1),
            CheckIn.Create(TestData.BobId, 4, "bob day5", Day5),
            CheckIn.Create(TestData.BobId, 5, "bob day10", Day10),
            CheckIn.Create(TestData.CarolId, 2, "carol day5", Day5));
        await db.SaveChangesAsync();
        return db;
    }

    private static GetCheckInsHandler Handler(ApplicationDbContext db) =>
        new(db, new GetCheckInsValidator());

    [Fact]
    public async Task Handle_FiltersByUserId()
    {
        await using var db = await SeededContextAsync();

        var result = await Handler(db).Handle(new GetCheckInsQuery { UserId = TestData.BobId });

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(3);
        result.Value.Items.Should().OnlyContain(c => c.UserId == TestData.BobId);
    }

    [Fact]
    public async Task Handle_FiltersByDateRange_Inclusive()
    {
        await using var db = await SeededContextAsync();

        var result = await Handler(db).Handle(new GetCheckInsQuery { From = Day5, To = Day10 });

        result.IsSuccess.Should().BeTrue();
        // Day5 (x2) + Day10 (x1); Day1 excluded.
        result.Value!.TotalCount.Should().Be(3);
        result.Value.Items.Should().NotContain(c => c.Notes == "bob day1");
    }

    [Fact]
    public async Task Handle_DateRangeTo_IncludesCheckInsLaterThatDay()
    {
        await using var db = TestData.CreateContext();
        db.Users.Add(TestData.Bob());
        db.CheckIns.Add(CheckIn.Create(
            TestData.BobId, 4, "late on day10", new DateTime(2026, 6, 10, 17, 30, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();

        // `to` is the calendar day (midnight); the 5:30pm check-in that day must still match.
        var to = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
        var result = await Handler(db).Handle(new GetCheckInsQuery { To = to });

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_OrdersByCreatedAtDescending()
    {
        await using var db = await SeededContextAsync();

        var result = await Handler(db).Handle(new GetCheckInsQuery { UserId = TestData.BobId });

        result.Value!.Items.Select(c => c.CreatedAt)
            .Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task Handle_AppliesPagination()
    {
        await using var db = await SeededContextAsync();

        var page1 = await Handler(db).Handle(new GetCheckInsQuery { Page = 1, PageSize = 2 });
        var page2 = await Handler(db).Handle(new GetCheckInsQuery { Page = 2, PageSize = 2 });

        page1.Value!.Items.Should().HaveCount(2);
        page1.Value.TotalCount.Should().Be(4);
        page1.Value.TotalPages.Should().Be(2);
        page1.Value.HasNextPage.Should().BeTrue();

        page2.Value!.Items.Should().HaveCount(2);
        page2.Value.HasNextPage.Should().BeFalse();

        page1.Value.Items.Select(i => i.Id)
            .Should().NotIntersectWith(page2.Value.Items.Select(i => i.Id));
    }

    [Fact]
    public async Task Handle_WithInvalidPageSize_ReturnsValidationError()
    {
        await using var db = await SeededContextAsync();

        var result = await Handler(db).Handle(new GetCheckInsQuery { PageSize = 0 });

        result.IsFailure.Should().BeTrue();
        result.ValidationErrors.Should().ContainKey(nameof(GetCheckInsQuery.PageSize));
    }
}
