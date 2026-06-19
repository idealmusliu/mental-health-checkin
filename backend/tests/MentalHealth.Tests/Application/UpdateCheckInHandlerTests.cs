using FluentAssertions;
using MentalHealth.Application.CheckIns.Commands.UpdateCheckIn;
using MentalHealth.Application.Common.Results;
using MentalHealth.Domain.Entities;
using MentalHealth.Infrastructure.Persistence;
using MentalHealth.Tests.Common;

namespace MentalHealth.Tests.Application;

public class UpdateCheckInHandlerTests
{
    private static readonly DateTime Now = new(2026, 6, 16, 9, 0, 0, DateTimeKind.Utc);

    private static UpdateCheckInHandler Handler(ApplicationDbContext db) =>
        new(db, new UpdateCheckInValidator(), new FakeDateTimeProvider(Now));

    [Fact]
    public async Task Handle_UpdatesOwnCheckIn()
    {
        await using var db = TestData.CreateContext();
        db.Users.Add(TestData.Bob());
        var checkIn = CheckIn.Create(TestData.BobId, 3, "before", Now.AddDays(-1));
        db.CheckIns.Add(checkIn);
        await db.SaveChangesAsync();

        var result = await Handler(db).Handle(
            new UpdateCheckInCommand(checkIn.Id, 5, "after", TestData.BobId));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Mood.Should().Be(5);
        (await db.CheckIns.FindAsync(checkIn.Id))!.Mood.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WhenCheckInBelongsToAnotherUser_ReturnsNotFoundAndDoesNotMutate()
    {
        await using var db = TestData.CreateContext();
        db.Users.AddRange(TestData.Bob(), TestData.Carol());
        var carolCheckIn = CheckIn.Create(TestData.CarolId, 2, "carol", Now.AddDays(-1));
        db.CheckIns.Add(carolCheckIn);
        await db.SaveChangesAsync();

        // Bob tries to edit Carol's check-in.
        var result = await Handler(db).Handle(
            new UpdateCheckInCommand(carolCheckIn.Id, 5, "hacked", RestrictToUserId: TestData.BobId));

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        (await db.CheckIns.FindAsync(carolCheckIn.Id))!.Mood.Should().Be(2);
    }
}
