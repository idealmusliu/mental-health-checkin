namespace MentalHealth.Application.CheckIns.Commands.UpdateCheckIn;

// RestrictToUserId, when set, requires the check-in to belong to that user (employee scoping).
public sealed record UpdateCheckInCommand(Guid Id, int Mood, string? Notes, Guid? RestrictToUserId = null);
