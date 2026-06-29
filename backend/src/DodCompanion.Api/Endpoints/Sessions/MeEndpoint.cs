using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Application.Common.Models;
using FastEndpoints;

namespace DodCompanion.Api.Endpoints.Sessions;

/// <summary>The current player's session context.</summary>
public sealed record MeResponse(string SessionId, string PlayerName);

/// <summary>GET /sessions/me — returns the signed-in player's session, used by the SPA to gate routes.</summary>
public sealed class MeEndpoint(IUserSession userSession) : EndpointWithoutRequest<ApiResponse<MeResponse>>
{
    public override void Configure()
    {
        Get("/sessions/me");
        Summary(s => s.Summary = "Get the current player's session context.");
        Tags("Sessions");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!userSession.IsAuthenticated || userSession.SessionId is null || userSession.PlayerName is null)
        {
            await Send.ResponseAsync(ApiResponse<MeResponse>.Fail("Not authenticated."), StatusCodes.Status401Unauthorized, ct);
            return;
        }

        var response = ApiResponse<MeResponse>.Ok(new MeResponse(userSession.SessionId, userSession.PlayerName));
        await Send.ResponseAsync(response, StatusCodes.Status200OK, ct);
    }
}
