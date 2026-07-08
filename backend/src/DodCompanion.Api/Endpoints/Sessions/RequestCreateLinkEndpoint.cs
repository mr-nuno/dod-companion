using DodCompanion.Application.Common.Models;
using DodCompanion.Application.Features.Sessions.RequestCreateLink;
using FastEndpoints;
using FluentValidation;
using MediatR;

namespace DodCompanion.Api.Endpoints.Sessions;

/// <summary>HTTP request body for requesting a create-session magic link.</summary>
public sealed record RequestCreateLinkRequest(string Email, string RoomName);

/// <summary>
/// POST /sessions/request-create — emails a single-use magic link to an allowlisted Game Master (SL).
/// Replaces the old shared host key. No cookie is issued here; the link is consumed via /sessions/consume-create.
/// </summary>
public sealed class RequestCreateLinkEndpoint(ISender sender)
    : Endpoint<RequestCreateLinkRequest, ApiResponse<bool>>
{
    public override void Configure()
    {
        Post("/sessions/request-create");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Email a create-session magic link to an allowlisted Game Master.";
            s.ExampleRequest = new RequestCreateLinkRequest("sl@example.com", "DRAGON");
        });
        Tags("Sessions");
    }

    public override async Task HandleAsync(RequestCreateLinkRequest req, CancellationToken ct)
    {
        var result = await sender.Send(new RequestCreateLinkCommand(req.Email, req.RoomName), ct);
        await Send.ResponseAsync(result.ToApiResponse(), result.ToHttpStatusCode(), ct);
    }
}

/// <summary>HTTP-level validation for the request-create-link record.</summary>
public sealed class RequestCreateLinkRequestValidator : Validator<RequestCreateLinkRequest>
{
    public RequestCreateLinkRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.");
        RuleFor(x => x.RoomName).NotEmpty().WithMessage("Room name is required.");
    }
}
