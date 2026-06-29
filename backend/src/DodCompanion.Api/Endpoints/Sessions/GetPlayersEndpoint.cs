using DodCompanion.Application.Common.Models;
using DodCompanion.Application.Features.Sessions.GetPlayers;
using FastEndpoints;
using MediatR;

namespace DodCompanion.Api.Endpoints.Sessions;

/// <summary>GET /sessions/players — returns all players and their character stats for the current session.</summary>
public sealed class GetPlayersEndpoint(ISender sender)
    : EndpointWithoutRequest<ApiResponse<GetPlayersResponse>>
{
    public override void Configure()
    {
        Get("/sessions/players");
        Summary(s => s.Summary = "Get all players and their character stats for the current session.");
        Tags("Sessions");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await sender.Send(new GetPlayersQuery(), ct);
        await Send.ResponseAsync(result.ToApiResponse(), result.ToHttpStatusCode(), ct);
    }
}
