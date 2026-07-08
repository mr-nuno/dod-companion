using Ardalis.Result;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Domain.Session;
using DodCompanion.Domain.SessionLink;
using FluentValidation;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

namespace DodCompanion.Application.Features.Sessions.ConsumeCreateLink;

/// <summary>
/// Consumes a single-use create-session magic link: creates the room, adds the Game Master (SL) as a DM
/// player, and returns the session so the endpoint can issue the auth cookie. Idempotency is enforced by
/// marking the link consumed — a second click fails.
/// </summary>
public sealed record ConsumeCreateLinkCommand(string Token) : IRequest<Result<SessionResult>>
{
    /// <summary>Display name the Game Master is signed in as.</summary>
    public const string GameMasterName = "SL";

    public sealed class Handler(IApplicationDbContext db, ITokenGenerator tokens, IDateTimeProvider clock)
        : IRequestHandler<ConsumeCreateLinkCommand, Result<SessionResult>>
    {
        public async Task<Result<SessionResult>> Handle(ConsumeCreateLinkCommand request, CancellationToken ct)
        {
            // WaitForNonStaleResults so a link created moments ago is immediately findable by its token.
            var link = await db.Query<SessionLinkAggregate>()
                .Customize(x => x.WaitForNonStaleResults())
                .FirstOrDefaultAsync(l => l.Token == request.Token, ct);

            var now = clock.UtcNow;
            if (link is null || !link.IsUsable(now))
            {
                return Result.NotFound("This link is invalid, already used, or expired.");
            }

            link.Consume(now);

            var roomCode = link.RoomName.Trim().ToUpperInvariant();
            var session = SessionAggregate.Create(roomCode, tokens.NewJoinToken(), now);
            session.Join(new PlayerInfo(GameMasterName, Kp: 0, UpptackFara: 0, FinnaDoldaTing: 0, IsDm: true));

            await db.StoreAsync(session, ct);
            await db.SaveChangesAsync(ct);

            return Result.Success(new SessionResult(session.Id, session.RoomCode, GameMasterName, session.JoinToken));
        }
    }

    public sealed class Validator : AbstractValidator<ConsumeCreateLinkCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Token).NotEmpty().WithMessage("A create-session token is required.");
        }
    }
}
