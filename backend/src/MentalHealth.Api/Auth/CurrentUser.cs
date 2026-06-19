using MentalHealth.Application.Common.Interfaces;
using MentalHealth.Domain.Enums;

namespace MentalHealth.Api.Auth;

// Demo auth: reads X-User-Id / X-User-Role headers set by the frontend. Not secure.
public sealed class CurrentUser : ICurrentUser
{
    public const string UserIdHeader = "X-User-Id";
    public const string UserRoleHeader = "X-User-Role";

    public CurrentUser(IHttpContextAccessor accessor)
    {
        var request = accessor.HttpContext?.Request;
        if (request is null) return;

        if (Guid.TryParse(request.Headers[UserIdHeader], out var userId))
            UserId = userId;

        if (Enum.TryParse<UserRole>(request.Headers[UserRoleHeader], ignoreCase: true, out var role))
            Role = role;
    }

    public Guid? UserId { get; }
    public UserRole? Role { get; }
    public bool IsAuthenticated => UserId.HasValue;
    public bool IsManager => Role == UserRole.Manager;
}
