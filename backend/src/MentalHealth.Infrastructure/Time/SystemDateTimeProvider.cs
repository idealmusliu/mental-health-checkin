using MentalHealth.Application.Common.Interfaces;

namespace MentalHealth.Infrastructure.Time;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
