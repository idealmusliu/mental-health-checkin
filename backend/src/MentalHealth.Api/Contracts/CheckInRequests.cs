namespace MentalHealth.Api.Contracts;

// The author comes from the authenticated caller (X-User-Id), not the body.
public sealed record CreateCheckInRequest(int Mood, string? Notes);

public sealed record UpdateCheckInRequest(int Mood, string? Notes);
