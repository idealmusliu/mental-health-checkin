using MentalHealth.Domain.Enums;

namespace MentalHealth.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
}
