using MentalHealth.Domain.Enums;

namespace MentalHealth.Application.Common.Interfaces;

// Demo auth: the caller's identity comes from request headers (see CurrentUser).
public interface ICurrentUser
{
    Guid? UserId { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    bool IsManager { get; }
}
