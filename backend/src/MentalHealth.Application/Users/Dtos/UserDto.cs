using MentalHealth.Domain.Entities;

namespace MentalHealth.Application.Users.Dtos;

public sealed record UserDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;

    public static UserDto FromEntity(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Role = user.Role.ToString()
    };
}
