using System.Linq.Expressions;
using MentalHealth.Domain.Entities;

namespace MentalHealth.Application.CheckIns.Dtos;

public sealed record CheckInDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public int Mood { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public static CheckInDto FromEntity(CheckIn checkIn) => new()
    {
        Id = checkIn.Id,
        UserId = checkIn.UserId,
        UserName = checkIn.User?.Name ?? string.Empty,
        Mood = checkIn.Mood,
        Notes = checkIn.Notes,
        CreatedAt = checkIn.CreatedAt,
        UpdatedAt = checkIn.UpdatedAt
    };

    // EF-translatable projection for list/detail queries (selects only needed columns).
    public static Expression<Func<CheckIn, CheckInDto>> Projection => checkIn => new CheckInDto
    {
        Id = checkIn.Id,
        UserId = checkIn.UserId,
        UserName = checkIn.User!.Name,
        Mood = checkIn.Mood,
        Notes = checkIn.Notes,
        CreatedAt = checkIn.CreatedAt,
        UpdatedAt = checkIn.UpdatedAt
    };
}
