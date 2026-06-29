using DodCompanion.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DodCompanion.Api.Hubs;

/// <summary>
/// Real-time timeline hub. Each connection joins a group keyed by its session id so broadcasts
/// only reach players in the same room.
/// </summary>
[Authorize]
public sealed class TimelineHub : Hub
{
    public const string LogEntryCreatedEvent = "LogEntryCreated";
    public const string PlayerJoinedEvent = "PlayerJoined";

    public override async Task OnConnectedAsync()
    {
        var sessionId = Context.User?.FindFirst(SessionClaimTypes.SessionId)?.Value;

        if (!string.IsNullOrEmpty(sessionId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        }

        await base.OnConnectedAsync();
    }
}
