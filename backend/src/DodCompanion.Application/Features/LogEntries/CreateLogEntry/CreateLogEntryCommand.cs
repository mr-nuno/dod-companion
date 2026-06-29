using Ardalis.Result;
using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Domain.LogEntry;
using FluentValidation;
using MediatR;

namespace DodCompanion.Application.Features.LogEntries.CreateLogEntry;

/// <summary>
/// Creates a timeline log entry for the current player's session, then broadcasts it to connected players.
/// Session and player are taken from the auth cookie via <see cref="IUserSession"/> — never from the request.
/// </summary>
public sealed record CreateLogEntryCommand(string Content) : IRequest<Result<LogEntryDto>>
{
    public sealed class Handler(
        IApplicationDbContext db,
        IUserSession userSession,
        IDateTimeProvider clock,
        ITimelineNotifier notifier) : IRequestHandler<CreateLogEntryCommand, Result<LogEntryDto>>
    {
        public async Task<Result<LogEntryDto>> Handle(CreateLogEntryCommand request, CancellationToken ct)
        {
            if (!userSession.IsAuthenticated || userSession.SessionId is null || userSession.PlayerName is null)
            {
                return Result.Unauthorized();
            }

            var entry = LogEntryAggregate.Create(
                userSession.SessionId,
                userSession.PlayerName,
                request.Content.Trim(),
                clock.UtcNow);

            await db.StoreAsync(entry, ct);
            await db.SaveChangesAsync(ct);

            var dto = entry.ToDto();
            await notifier.LogEntryCreatedAsync(dto.SessionId, dto, ct);

            return Result.Success(dto);
        }
    }

    public sealed class Validator : AbstractValidator<CreateLogEntryCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Log entry content is required.")
                .MaximumLength(4000).WithMessage("Log entry must be 4000 characters or fewer.");
        }
    }
}
