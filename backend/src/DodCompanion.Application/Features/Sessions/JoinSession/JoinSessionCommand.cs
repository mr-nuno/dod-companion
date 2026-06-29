using Ardalis.Result;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Domain.Session;
using FluentValidation;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

namespace DodCompanion.Application.Features.Sessions.JoinSession;

/// <summary>
/// Joins a player to a session identified by room code. The session is auto-created on first join
/// of an unknown code; later joins of the same code attach to it. Cookie sign-in happens in the endpoint.
/// </summary>
public sealed record JoinSessionCommand(string RoomCode, string PlayerName) : IRequest<Result<JoinSessionResponse>>
{
    public sealed class Handler(IApplicationDbContext db, IDateTimeProvider clock)
        : IRequestHandler<JoinSessionCommand, Result<JoinSessionResponse>>
    {
        public async Task<Result<JoinSessionResponse>> Handle(JoinSessionCommand request, CancellationToken ct)
        {
            var roomCode = Normalize(request.RoomCode);
            var playerName = request.PlayerName.Trim();

            var session = await db.Query<SessionAggregate>()
                .FirstOrDefaultAsync(s => s.RoomCode == roomCode, ct);

            if (session is null)
            {
                session = SessionAggregate.Create(roomCode, clock.UtcNow);
                await db.StoreAsync(session, ct);
            }

            session.Join(playerName);
            await db.SaveChangesAsync(ct);

            return Result.Success(new JoinSessionResponse(session.Id, session.RoomCode, playerName));
        }

        private static string Normalize(string roomCode) => roomCode.Trim().ToUpperInvariant();
    }

    public sealed class Validator : AbstractValidator<JoinSessionCommand>
    {
        public Validator()
        {
            RuleFor(x => x.RoomCode)
                .NotEmpty().WithMessage("Room code is required.")
                .MaximumLength(32).WithMessage("Room code must be 32 characters or fewer.");

            RuleFor(x => x.PlayerName)
                .NotEmpty().WithMessage("Player name is required.")
                .MaximumLength(64).WithMessage("Player name must be 64 characters or fewer.");
        }
    }
}

/// <summary>Identifies the joined session and the player, used to issue the auth cookie.</summary>
public sealed record JoinSessionResponse(string SessionId, string RoomCode, string PlayerName);
