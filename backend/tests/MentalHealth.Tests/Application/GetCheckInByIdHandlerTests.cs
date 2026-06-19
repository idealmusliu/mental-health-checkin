using FluentAssertions;
using MentalHealth.Application.CheckIns.Queries.GetCheckInById;
using MentalHealth.Application.Common.Results;
using MentalHealth.Domain.Entities;
using MentalHealth.Tests.Common;

namespace MentalHealth.Tests.Application;

public class GetCheckInByIdHandlerTests
{
    private static readonly DateTime When = new(2026, 6, 16, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handle_ReturnsCheckIn_WhenNotRestricted()
    {
        await using var db = TestData.CreateContext();
        db.Users.Add(TestData.Bob());
        var checkIn = CheckIn.Create(TestData.BobId, 4, "note", When);
        db.CheckIns.Add(checkIn);
        await db.SaveChangesAsync();

        var result = await new GetCheckInByIdHandler(db).Handle(new GetCheckInByIdQuery(checkIn.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(checkIn.Id);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenRestrictedToDifferentUser()
    {
        await using var db = TestData.CreateContext();
        db.Users.AddRange(TestData.Bob(), TestData.Carol());
        var carolCheckIn = CheckIn.Create(TestData.CarolId, 4, "carol", When);
        db.CheckIns.Add(carolCheckIn);
        await db.SaveChangesAsync();

        // Bob requests Carol's check-in scoped to himself.
        var result = await new GetCheckInByIdHandler(db).Handle(
            new GetCheckInByIdQuery(carolCheckIn.Id, RestrictToUserId: TestData.BobId));

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }
}
