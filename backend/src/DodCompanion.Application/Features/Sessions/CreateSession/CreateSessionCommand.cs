using Ardalis.Result;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Domain.Session;
using FluentValidation;
using MediatR;

namespace DodCompanion.Application.Features.Sessions.CreateSession;

/// <summary>
/// Provisions a new room for a host-supplied name, generating an unguessable join token. No player is
/// added and no cookie is issued here — the host enters their name and joins via the token (like every
/// other player) once the QR code is shown. Host-key authorization happens in the endpoint.
/// </summary>
public sealed record CreateSessionCommand(string RoomName) : IRequest<Result<CreateSessionResult>>
{
    public sealed class Handler(IApplicationDbContext db, ITokenGenerator tokens, IDateTimeProvider clock)
        : IRequestHandler<CreateSessionCommand, Result<CreateSessionResult>>
    {
        public async Task<Result<CreateSessionResult>> Handle(CreateSessionCommand request, CancellationToken ct)
        {
            var roomCode = request.RoomName.Trim().ToUpperInvariant();

            var session = SessionAggregate.Create(roomCode, tokens.NewJoinToken(), clock.UtcNow);
            await db.StoreAsync(session, ct);
            await db.SaveChangesAsync(ct);

            return Result.Success(new CreateSessionResult(session.Id, session.RoomCode, session.JoinToken));
        }
    }

    public sealed class Validator : AbstractValidator<CreateSessionCommand>
    {
        public Validator()
        {
            RuleFor(x => x.RoomName)
                .NotEmpty().WithMessage("Room name is required.")
                .MaximumLength(32).WithMessage("Room name must be 32 characters or fewer.");
        }
    }
}

/// <summary>A newly provisioned room. Carries the join token so the client can render the shareable QR.</summary>
public sealed record CreateSessionResult(string SessionId, string RoomCode, string JoinToken);
