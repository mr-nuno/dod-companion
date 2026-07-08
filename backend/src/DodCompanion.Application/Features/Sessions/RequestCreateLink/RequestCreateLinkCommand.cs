using Ardalis.Result;
using DodCompanion.Application.Common.Configuration;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Domain.SessionLink;
using FluentValidation;
using MediatR;

namespace DodCompanion.Application.Features.Sessions.RequestCreateLink;

/// <summary>
/// Issues a single-use "create session" magic link and emails it to an allowlisted Game Master (SL).
/// Replaces the old shared host key. The link is only sent when the email is on the configured allowlist.
/// </summary>
public sealed record RequestCreateLinkCommand(string Email, string RoomName) : IRequest<Result<bool>>
{
    public sealed class Handler(
        IApplicationDbContext db,
        ITokenGenerator tokens,
        IDateTimeProvider clock,
        IEmailSender email,
        SessionOptions options) : IRequestHandler<RequestCreateLinkCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(RequestCreateLinkCommand request, CancellationToken ct)
        {
            var toEmail = request.Email.Trim();

            if (!options.IsAllowed(toEmail))
            {
                return Result.Error("This email is not authorized to create sessions.");
            }

            var now = clock.UtcNow;
            var link = SessionLinkAggregate.Create(
                tokens.NewJoinToken(),
                toEmail,
                request.RoomName.Trim(),
                now,
                now.AddMinutes(options.MagicLinkTtlMinutes));

            await db.StoreAsync(link, ct);
            await db.SaveChangesAsync(ct);

            var url = $"{options.AppBaseUrl.TrimEnd('/')}/create?token={link.Token}";
            await email.SendAsync(toEmail, "Your DoD Companion session link", BuildHtml(link.RoomName, url, options.MagicLinkTtlMinutes), ct);

            return Result.Success(true);
        }

        private static string BuildHtml(string roomName, string url, int ttlMinutes) =>
            $"""
            <div style="font-family:system-ui,sans-serif;max-width:480px;margin:0 auto;padding:24px">
              <h1 style="color:#e31c23;margin:0 0 8px">DoD Companion</h1>
              <p>You requested to create the session <strong>{System.Net.WebUtility.HtmlEncode(roomName)}</strong>.</p>
              <p>Click the button below to create the room and sign in as Game Master (SL):</p>
              <p style="margin:24px 0">
                <a href="{url}" style="background:#e31c23;color:#fff;padding:12px 24px;border-radius:8px;text-decoration:none;font-weight:bold">Create session</a>
              </p>
              <p style="color:#666;font-size:13px">This link expires in {ttlMinutes} minutes and can be used once. If you didn't request it, you can ignore this email.</p>
            </div>
            """;
    }

    public sealed class Validator : AbstractValidator<RequestCreateLinkCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email is required.");

            RuleFor(x => x.RoomName)
                .NotEmpty().WithMessage("Room name is required.")
                .MaximumLength(32).WithMessage("Room name must be 32 characters or fewer.");
        }
    }
}
