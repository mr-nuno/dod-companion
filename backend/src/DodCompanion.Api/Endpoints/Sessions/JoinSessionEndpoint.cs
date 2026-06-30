using DodCompanion.Api.Auth;
using DodCompanion.Application.Common.Models;
using DodCompanion.Application.Features.Sessions;
using DodCompanion.Application.Features.Sessions.JoinSession;
using FastEndpoints;
using FluentValidation;
using MediatR;

namespace DodCompanion.Api.Endpoints.Sessions;

/// <summary>HTTP request body for joining a session via its join token (from the QR code).</summary>
public sealed record JoinSessionRequest(
    string JoinToken,
    string PlayerName,
    int Kp,
    int UpptackFara,
    int FinnaDoldaTing,
    bool IsDm = false);

/// <summary>POST /sessions/join — joins an existing session by its join token and issues the auth cookie.</summary>
public sealed class JoinSessionEndpoint(ISender sender)
    : Endpoint<JoinSessionRequest, ApiResponse<SessionResult>>
{
    public override void Configure()
    {
        Post("/sessions/join");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Join a session via its join token.";
            s.Description = "The join token comes from the room's QR code / invite link. Unknown tokens are "
                + "rejected — there is no way to join by guessing a room name. On success an HttpOnly "
                + "authentication cookie is issued.";
            s.ExampleRequest = new JoinSessionRequest("k3J9-xQ2...", "Aragorn", 14, 10, 8);
        });
        Tags("Sessions");
    }

    public override async Task HandleAsync(JoinSessionRequest req, CancellationToken ct)
    {
        var result = await sender.Send(
            new JoinSessionCommand(req.JoinToken, req.PlayerName, req.Kp, req.UpptackFara, req.FinnaDoldaTing, req.IsDm), ct);

        if (result.IsSuccess)
        {
            await HttpContext.IssueSessionCookieAsync(result.Value);
        }

        await Send.ResponseAsync(result.ToApiResponse(), result.ToHttpStatusCode(), ct);
    }
}

/// <summary>HTTP-level validation for the join request record.</summary>
public sealed class JoinSessionRequestValidator : Validator<JoinSessionRequest>
{
    public JoinSessionRequestValidator()
    {
        RuleFor(x => x.JoinToken).NotEmpty().WithMessage("A join link is required.");
        RuleFor(x => x.PlayerName).NotEmpty().WithMessage("Player name is required.");
        RuleFor(x => x.Kp).GreaterThan(0).When(x => !x.IsDm).WithMessage("KP must be a positive number.");
        RuleFor(x => x.UpptackFara).GreaterThan(0).When(x => !x.IsDm).WithMessage("Upptäck fara must be a positive number.");
        RuleFor(x => x.FinnaDoldaTing).GreaterThan(0).When(x => !x.IsDm).WithMessage("Finna dolda ting must be a positive number.");
    }
}
