using FluentAssertions;
using MentalHealth.Application.CheckIns.Commands.CreateCheckIn;
using MentalHealth.Application.Common.Results;
using MentalHealth.Infrastructure.Persistence;
using MentalHealth.Tests.Common;

namespace MentalHealth.Tests.Application;

public class CreateCheckInHandlerTests
{
    private static readonly DateTime Now = new(2026, 6, 16, 9, 0, 0, DateTimeKind.Utc);

    private static CreateCheckInHandler CreateHandler(ApplicationDbContext db) =>
        new(db, new CreateCheckInValidator(), new FakeDateTimeProvider(Now));

    [Fact]
    public async Task Handle_WithValidCommand_PersistsCheckInAndReturnsDto()
    {
        await using var db = TestData.CreateContext();
        db.Users.Add(TestData.Bob());
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var command = new CreateCheckInCommand(TestData.BobId, Mood: 4, Notes: "Feeling good");

        var result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Mood.Should().Be(4);
        result.Value.Notes.Should().Be("Feeling good");
        result.Value.UserName.Should().Be("Bob Employee");
        result.Value.CreatedAt.Should().Be(Now);

        db.CheckIns.Should().ContainSingle()
            .Which.UserId.Should().Be(TestData.BobId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task Handle_WithMoodOutOfRange_ReturnsValidationError(int invalidMood)
    {
        await using var db = TestData.CreateContext();
        db.Users.Add(TestData.Bob());
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var command = new CreateCheckInCommand(TestData.BobId, invalidMood, Notes: null);

        var result = await handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.ValidationErrors.Should().ContainKey(nameof(CreateCheckInCommand.Mood));
        db.CheckIns.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNotFound()
    {
        await using var db = TestData.CreateContext();
        var handler = CreateHandler(db);
        var command = new CreateCheckInCommand(TestData.BobId, Mood: 3, Notes: null);

        var result = await handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        db.CheckIns.Should().BeEmpty();
    }
}
