using Ardalis.Result;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Domain.LogEntry;
using FluentValidation;
using MediatR;

namespace DodCompanion.Application.Features.LogEntries.DeleteLogEntry;

/// <summary>
/// Deletes a timeline entry, then broadcasts the removal to connected players.
/// Only the entry's original author (same session + player name) may delete it.
/// Session and player are taken from the auth cookie via <see cref="IUserSession"/> — never from the request.
/// </summary>
public sealed record DeleteLogEntryCommand(string Id) : IRequest<Result<bool>>
{
    public sealed class Handler(
        IApplicationDbContext db,
        IUserSession userSession,
        ITimelineNotifier notifier) : IRequestHandler<DeleteLogEntryCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteLogEntryCommand request, CancellationToken ct)
        {
            if (!userSession.IsAuthenticated || userSession.SessionId is null || userSession.PlayerName is null)
            {
                return Result.Unauthorized();
            }

            var entry = await db.LoadAsync<LogEntryAggregate>(request.Id, ct);
            if (entry is null)
            {
                return Result.NotFound();
            }

            if (entry.SessionId != userSession.SessionId || entry.PlayerName != userSession.PlayerName)
            {
                return Result.Forbidden();
            }

            var sessionId = entry.SessionId;

            db.Delete(entry);
            await db.SaveChangesAsync(ct);

            await notifier.LogEntryDeletedAsync(sessionId, request.Id, ct);

            return Result.Success(true);
        }
    }

    public sealed class Validator : AbstractValidator<DeleteLogEntryCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Log entry id is required.");
        }
    }
}
