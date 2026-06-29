using System.Text;
using Ardalis.Result;
using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Domain.LogEntry;
using DodCompanion.Domain.Session;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

namespace DodCompanion.Application.Features.SessionSummary;

/// <summary>
/// Generates a markdown summary of the session's timeline and persists it to the database.
/// Re-generating overwrites the existing summary for the session.
/// </summary>
public sealed record GenerateSummaryCommand : IRequest<Result<SessionSummaryDto>>
{
    public sealed class Handler(IApplicationDbContext db, IUserSession userSession, IDateTimeProvider clock)
        : IRequestHandler<GenerateSummaryCommand, Result<SessionSummaryDto>>
    {
        public async Task<Result<SessionSummaryDto>> Handle(GenerateSummaryCommand request, CancellationToken ct)
        {
            if (!userSession.IsAuthenticated || userSession.SessionId is null)
            {
                return Result.Unauthorized();
            }

            var sessionId = userSession.SessionId;

            var session = await db.LoadAsync<SessionAggregate>(sessionId, ct);
            if (session is null)
            {
                return Result.NotFound("Session not found.");
            }

            var entries = await db.Query<LogEntryAggregate>()
                .Where(e => e.SessionId == sessionId)
                .OrderBy(e => e.Timestamp)
                .ToListAsync(ct);

            var filteredEntries = entries
                .Where(e => !e.Tags.Contains("info", StringComparer.OrdinalIgnoreCase))
                .ToList();

            var content = BuildMarkdown(session.RoomCode, filteredEntries, clock.UtcNow);

            // Upsert: overwrite if a summary already exists for this session.
            var existing = await db.Query<Domain.SessionSummary.SessionSummaryAggregate>()
                .FirstOrDefaultAsync(s => s.SessionId == sessionId, ct);

            Domain.SessionSummary.SessionSummaryAggregate summary;
            if (existing is not null)
            {
                existing.Regenerate(content, filteredEntries.Count, clock.UtcNow);
                summary = existing;
            }
            else
            {
                summary = Domain.SessionSummary.SessionSummaryAggregate.Create(
                    sessionId, session.RoomCode, content, filteredEntries.Count, clock.UtcNow);
                await db.StoreAsync(summary, ct);
            }

            await db.SaveChangesAsync(ct);

            return Result.Success(ToDto(summary));
        }

        private static string BuildMarkdown(string roomCode, List<LogEntryAggregate> entries, DateTimeOffset generatedAt)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# Session: {roomCode}");
            sb.AppendLine();
            sb.AppendLine($"*Generated: {generatedAt:yyyy-MM-dd HH:mm} UTC*");
            sb.AppendLine();
            sb.AppendLine("## Timeline");
            sb.AppendLine();

            foreach (var entry in entries)
            {
                sb.AppendLine($"### {entry.Timestamp:HH:mm} — {entry.PlayerName}");
                if (entry.Tags.Count > 0)
                {
                    sb.AppendLine($"**Tags:** {string.Join(", ", entry.Tags)}");
                    sb.AppendLine();
                }
                sb.AppendLine(entry.Content);
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        private static SessionSummaryDto ToDto(Domain.SessionSummary.SessionSummaryAggregate s) =>
            new(s.Id, s.SessionId, s.RoomCode, s.Content, s.EntryCount, s.GeneratedAt);
    }
}
