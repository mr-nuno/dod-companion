using System.Security.Claims;
using DodCompanion.Application.Features.Sessions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DodCompanion.Api.Auth;

/// <summary>Issues the HttpOnly authentication cookie for a created/joined session.</summary>
public static class SessionSignIn
{
    public static Task IssueSessionCookieAsync(this HttpContext httpContext, SessionResult session)
    {
        var claims = new List<Claim>
        {
            new(SessionClaimTypes.SessionId, session.SessionId),
            new(SessionClaimTypes.PlayerName, session.PlayerName),
            new(SessionClaimTypes.RoomCode, session.RoomCode),
            new(SessionClaimTypes.JoinToken, session.JoinToken),
            new(ClaimTypes.Name, session.PlayerName),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        return httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}
