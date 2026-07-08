using DodCompanion.Api.Auth;
using DodCompanion.Application.Common.Models;
using DodCompanion.Application.Features.Sessions;
using DodCompanion.Application.Features.Sessions.ConsumeCreateLink;
using FastEndpoints;
using FluentValidation;
using MediatR;

namespace DodCompanion.Api.Endpoints.Sessions;

/// <summary>HTTP request body for consuming a create-session magic link.</summary>
public sealed record ConsumeCreateLinkRequest(string Token);

/// <summary>
/// POST /sessions/consume-create — consumes a magic link, creating the room and signing the clicker in as
/// Game Master (SL) via the HttpOnly auth cookie. The link is single-use.
/// </summary>
public sealed class ConsumeCreateLinkEndpoint(ISender sender)
    : Endpoint<ConsumeCreateLinkRequest, ApiResponse<SessionResult>>
{
    public override void Configure()
    {
        Post("/sessions/consume-create");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Consume a create-session magic link (creates the room and signs in as SL).";
            s.ExampleRequest = new ConsumeCreateLinkRequest("k3J9-xQ2...");
        });
        Tags("Sessions");
    }

    public override async Task HandleAsync(ConsumeCreateLinkRequest req, CancellationToken ct)
    {
        var result = await sender.Send(new ConsumeCreateLinkCommand(req.Token), ct);

        if (result.IsSuccess)
        {
            await HttpContext.IssueSessionCookieAsync(result.Value);
        }

        await Send.ResponseAsync(result.ToApiResponse(), result.ToHttpStatusCode(), ct);
    }
}

/// <summary>HTTP-level validation for the consume-create-link record.</summary>
public sealed class ConsumeCreateLinkRequestValidator : Validator<ConsumeCreateLinkRequest>
{
    public ConsumeCreateLinkRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("A create-session token is required.");
    }
}
