using System.Security.Cryptography;
using System.Text;
using DodCompanion.Application.Common.Models;
using DodCompanion.Application.Features.Sessions.CreateSession;
using FastEndpoints;
using FluentValidation;
using MediatR;

namespace DodCompanion.Api.Endpoints.Sessions;

/// <summary>HTTP request body for provisioning a room. Requires the shared host key.</summary>
public sealed record CreateSessionRequest(string RoomName, string HostKey);

/// <summary>
/// POST /sessions/create — provisions a new room (gated by the shared host key) and returns its join
/// token. No cookie is issued: the host then enters a player name and joins via the token (POST
/// /sessions/join), exactly like anyone scanning the QR. The configured key lives in
/// <c>Sessions:CreateKey</c> (env var <c>Sessions__CreateKey</c> in production).
/// </summary>
public sealed class CreateSessionEndpoint(ISender sender, IConfiguration configuration)
    : Endpoint<CreateSessionRequest, ApiResponse<CreateSessionResult>>
{
    public override void Configure()
    {
        Post("/sessions/create");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Provision a new room (host only).";
            s.Description = "Requires the shared host key. Generates an unguessable join token used by the "
                + "QR code so players (including the host) can join. No cookie is issued here.";
            s.ExampleRequest = new CreateSessionRequest("DRAGON", "••••••");
        });
        Tags("Sessions");
    }

    public override async Task HandleAsync(CreateSessionRequest req, CancellationToken ct)
    {
        if (!IsValidHostKey(req.HostKey))
        {
            await Send.ResponseAsync(
                ApiResponse<CreateSessionResult>.Fail("Invalid host key."),
                StatusCodes.Status403Forbidden,
                ct);
            return;
        }

        var result = await sender.Send(new CreateSessionCommand(req.RoomName), ct);

        await Send.ResponseAsync(result.ToApiResponse(), result.ToHttpStatusCode(), ct);
    }

    // Constant-time comparison against the configured key. Fails closed: if no key is configured,
    // creation is denied entirely rather than left open.
    private bool IsValidHostKey(string? provided)
    {
        var configured = configuration["Sessions:CreateKey"];
        if (string.IsNullOrEmpty(configured) || string.IsNullOrEmpty(provided))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(configured),
            Encoding.UTF8.GetBytes(provided));
    }
}

/// <summary>HTTP-level validation for the create request record.</summary>
public sealed class CreateSessionRequestValidator : Validator<CreateSessionRequest>
{
    public CreateSessionRequestValidator()
    {
        RuleFor(x => x.RoomName).NotEmpty().WithMessage("Room name is required.");
        RuleFor(x => x.HostKey).NotEmpty().WithMessage("Host key is required.");
    }
}
