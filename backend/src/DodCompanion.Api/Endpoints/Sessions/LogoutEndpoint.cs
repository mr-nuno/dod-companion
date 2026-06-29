using DodCompanion.Application.Common.Models;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DodCompanion.Api.Endpoints.Sessions;

/// <summary>POST /sessions/logout — clears the authentication cookie.</summary>
public sealed class LogoutEndpoint : EndpointWithoutRequest<ApiResponse<object>>
{
    public override void Configure()
    {
        Post("/sessions/logout");
        Summary(s => s.Summary = "Sign out and clear the session cookie.");
        Tags("Sessions");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await Send.ResponseAsync(ApiResponse<object>.Ok(new object()), StatusCodes.Status200OK, ct);
    }
}
