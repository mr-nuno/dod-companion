using DodCompanion.Application.Common.Interfaces;

namespace DodCompanion.Api.Auth;

/// <summary>Reads the current player's session context from the authentication cookie claims.</summary>
public sealed class CookieUserSession(IHttpContextAccessor httpContextAccessor) : IUserSession
{
    private readonly System.Security.Claims.ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public string? SessionId => _user?.FindFirst(SessionClaimTypes.SessionId)?.Value;

    public string? PlayerName => _user?.FindFirst(SessionClaimTypes.PlayerName)?.Value;

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated ?? false;
}
