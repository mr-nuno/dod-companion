using Ardalis.Result;
using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Domain.LogEntry;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

// ToDto mapping lives in the LogEntries feature root.
using DodCompanion.Application.Features.LogEntries;

namespace DodCompanion.Application.Features.LogEntries.GetTimeline;

/// <summary>
/// Returns the chronological timeline for the current player's session (for the initial load;
/// subsequent entries arrive via SignalR).
/// </summary>
public sealed record GetTimelineQuery : IRequest<Result<TimelineResponse>>
{
    public sealed class Handler(IApplicationDbContext db, IUserSession userSession)
        : IRequestHandler<GetTimelineQuery, Result<TimelineResponse>>
    {
        public async Task<Result<TimelineResponse>> Handle(GetTimelineQuery request, CancellationToken ct)
        {
            if (!userSession.IsAuthenticated || userSession.SessionId is null)
            {
                return Result.Unauthorized();
            }

            var sessionId = userSession.SessionId;

            // RavenDB can't project into a positional-record constructor, so materialize then map.
            var entries = await db.Query<LogEntryAggregate>()
                .Where(e => e.SessionId == sessionId)
                .OrderBy(e => e.Timestamp)
                .ToListAsync(ct);

            return Result.Success(new TimelineResponse(entries.Select(e => e.ToDto()).ToList()));
        }
    }
}

public sealed record TimelineResponse(IReadOnlyList<LogEntryDto> Entries);
