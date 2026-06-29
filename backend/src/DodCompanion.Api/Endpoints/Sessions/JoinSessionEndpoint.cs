using System.Security.Claims;
using DodCompanion.Api.Auth;
using DodCompanion.Application.Common.Models;
using DodCompanion.Application.Features.Sessions.JoinSession;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DodCompanion.Api.Endpoints.Sessions;

/// <summary>HTTP request body for joining a session.</summary>
public sealed record JoinSessionRequest(string RoomCode, string PlayerName);

/// <summary>POST /sessions/join — auto-creates or joins a session and issues the auth cookie.</summary>
public sealed class JoinSessionEndpoint(ISender sender)
    : Endpoint<JoinSessionRequest, ApiResponse<JoinSessionResponse>>
{
    public override void Configure()
    {
        Post("/sessions/join");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Join (or create) a session by room code.";
            s.Description = "Menti-style entry: a room code plus a player name. Unknown codes create a new session. "
                + "On success an HttpOnly authentication cookie is issued.";
            s.ExampleRequest = new JoinSessionRequest("DRAGON", "Aragorn");
        });
        Tags("Sessions");
    }

    public override async Task HandleAsync(JoinSessionRequest req, CancellationToken ct)
    {
        var result = await sender.Send(new JoinSessionCommand(req.RoomCode, req.PlayerName), ct);

        if (result.IsSuccess)
        {
            await SignInAsync(result.Value, ct);
        }

        await Send.ResponseAsync(result.ToApiResponse(), result.ToHttpStatusCode(), ct);
    }

    private async Task SignInAsync(JoinSessionResponse session, CancellationToken ct)
    {
        var claims = new List<Claim>
        {
            new(SessionClaimTypes.SessionId, session.SessionId),
            new(SessionClaimTypes.PlayerName, session.PlayerName),
            new(ClaimTypes.Name, session.PlayerName),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}

/// <summary>HTTP-level validation for the join request record.</summary>
public sealed class JoinSessionRequestValidator : Validator<JoinSessionRequest>
{
    public JoinSessionRequestValidator()
    {
        RuleFor(x => x.RoomCode).NotEmpty().WithMessage("Room code is required.");
        RuleFor(x => x.PlayerName).NotEmpty().WithMessage("Player name is required.");
    }
}
