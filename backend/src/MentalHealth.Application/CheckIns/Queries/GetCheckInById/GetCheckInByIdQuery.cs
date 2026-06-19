namespace MentalHealth.Application.CheckIns.Queries.GetCheckInById;

// RestrictToUserId, when set, limits the lookup to that user's own check-in (employee scoping).
public sealed record GetCheckInByIdQuery(Guid Id, Guid? RestrictToUserId = null);
