using Ardalis.Result;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Application.Features.Sessions;
using DodCompanion.Domain.Session;
using FluentValidation;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

namespace DodCompanion.Application.Features.Sessions.JoinSession;

/// <summary>
/// Joins a player to an existing session identified by its unguessable join token (carried in the QR code).
/// Unknown tokens are rejected — there is no auto-create. Cookie sign-in happens in the endpoint.
/// </summary>
public sealed record JoinSessionCommand(string JoinToken, string PlayerName) : IRequest<Result<SessionResult>>
{
    public sealed class Handler(IApplicationDbContext db)
        : IRequestHandler<JoinSessionCommand, Result<SessionResult>>
    {
        public async Task<Result<SessionResult>> Handle(JoinSessionCommand request, CancellationToken ct)
        {
            var joinToken = request.JoinToken.Trim();
            var playerName = request.PlayerName.Trim();

            // Wait for the index to catch up: a room created moments earlier (in a separate request)
            // must be immediately joinable — including by the host who just created it.
            var session = await db.Query<SessionAggregate>()
                .Customize(x => x.WaitForNonStaleResults())
                .FirstOrDefaultAsync(s => s.JoinToken == joinToken, ct);

            if (session is null)
            {
                return Result.NotFound("Invalid or expired join link.");
            }

            session.Join(playerName);
            await db.SaveChangesAsync(ct);

            return Result.Success(new SessionResult(session.Id, session.RoomCode, playerName, session.JoinToken));
        }
    }

    public sealed class Validator : AbstractValidator<JoinSessionCommand>
    {
        public Validator()
        {
            RuleFor(x => x.JoinToken)
                .NotEmpty().WithMessage("A join link is required.");

            RuleFor(x => x.PlayerName)
                .NotEmpty().WithMessage("Player name is required.")
                .MaximumLength(64).WithMessage("Player name must be 64 characters or fewer.");
        }
    }
}
