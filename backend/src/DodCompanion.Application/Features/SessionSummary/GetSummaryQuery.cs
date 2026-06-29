using Ardalis.Result;
using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Interfaces;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

namespace DodCompanion.Application.Features.SessionSummary;

/// <summary>Returns the stored summary for the current session, or 404 if none has been generated yet.</summary>
public sealed record GetSummaryQuery : IRequest<Result<SessionSummaryDto>>
{
    public sealed class Handler(IApplicationDbContext db, IUserSession userSession)
        : IRequestHandler<GetSummaryQuery, Result<SessionSummaryDto>>
    {
        public async Task<Result<SessionSummaryDto>> Handle(GetSummaryQuery request, CancellationToken ct)
        {
            if (!userSession.IsAuthenticated || userSession.SessionId is null)
            {
                return Result.Unauthorized();
            }

            var sessionId = userSession.SessionId;

            var summary = await db.Query<Domain.SessionSummary.SessionSummaryAggregate>()
                .FirstOrDefaultAsync(s => s.SessionId == sessionId, ct);

            if (summary is null)
            {
                return Result.NotFound("No summary has been generated for this session yet.");
            }

            return Result.Success(new SessionSummaryDto(
                summary.Id, summary.SessionId, summary.RoomCode,
                summary.Content, summary.EntryCount, summary.GeneratedAt));
        }
    }
}
