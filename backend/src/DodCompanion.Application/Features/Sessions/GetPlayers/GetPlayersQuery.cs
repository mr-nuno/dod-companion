using Ardalis.Result;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Domain.Session;
using MediatR;

namespace DodCompanion.Application.Features.Sessions.GetPlayers;

public sealed record GetPlayersQuery : IRequest<Result<GetPlayersResponse>>;

public sealed record GetPlayersResponse(IReadOnlyList<PlayerInfo> Players);

public sealed class GetPlayersHandler(IApplicationDbContext db, IUserSession userSession)
    : IRequestHandler<GetPlayersQuery, Result<GetPlayersResponse>>
{
    public async Task<Result<GetPlayersResponse>> Handle(GetPlayersQuery request, CancellationToken ct)
    {
        if (!userSession.IsAuthenticated || userSession.SessionId is null)
        {
            return Result.Unauthorized();
        }

        var session = await db.LoadAsync<SessionAggregate>(userSession.SessionId, ct);
        if (session is null)
        {
            return Result.NotFound("Session not found.");
        }

        return Result.Success(new GetPlayersResponse(session.Players));
    }
}
