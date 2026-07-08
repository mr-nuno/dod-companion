using Ardalis.Result;
using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Domain.LogEntry;
using FluentValidation;
using MediatR;

namespace DodCompanion.Application.Features.LogEntries.UpdateLogEntry;

/// <summary>
/// Revises an existing timeline entry, then broadcasts the change to connected players.
/// Only the entry's original author (same session + player name) may edit it.
/// Session and player are taken from the auth cookie via <see cref="IUserSession"/> — never from the request.
/// </summary>
public sealed record UpdateLogEntryCommand(string Id, string Title, string Content, IReadOnlyList<string> Tags) : IRequest<Result<LogEntryDto>>
{
    public sealed class Handler(
        IApplicationDbContext db,
        IUserSession userSession,
        IDateTimeProvider clock,
        ITimelineNotifier notifier) : IRequestHandler<UpdateLogEntryCommand, Result<LogEntryDto>>
    {
        public async Task<Result<LogEntryDto>> Handle(UpdateLogEntryCommand request, CancellationToken ct)
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

            entry.Edit(request.Title.Trim(), request.Content.Trim(), request.Tags, clock.UtcNow);
            await db.SaveChangesAsync(ct);

            var dto = entry.ToDto();
            await notifier.LogEntryUpdatedAsync(dto.SessionId, dto, ct);

            return Result.Success(dto);
        }
    }

    public sealed class Validator : AbstractValidator<UpdateLogEntryCommand>
    {
        private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
            { "Strid", "Loot", "Event", "Anteckning", "Dödsfall", "info" };

        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Log entry id is required.");

            RuleFor(x => x.Title)
                .MaximumLength(120).WithMessage("Title must be 120 characters or fewer.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Log entry content is required.")
                .MaximumLength(4000).WithMessage("Log entry must be 4000 characters or fewer.");

            RuleFor(x => x.Tags)
                .Must(tags => tags.Count <= 5).WithMessage("A log entry may have at most 5 tags.")
                .Must(tags => tags.All(t => AllowedTags.Contains(t))).WithMessage("One or more tags are not valid.");
        }
    }
}
