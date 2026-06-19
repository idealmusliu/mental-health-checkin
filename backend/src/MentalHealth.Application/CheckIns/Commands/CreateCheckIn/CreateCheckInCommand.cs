namespace MentalHealth.Application.CheckIns.Commands.CreateCheckIn;

public sealed record CreateCheckInCommand(Guid UserId, int Mood, string? Notes);
