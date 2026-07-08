using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Domain.Session;
using Microsoft.AspNetCore.SignalR;

namespace DodCompanion.Api.Hubs;

/// <summary>SignalR implementation of the timeline notifier seam — broadcasts to the session's group.</summary>
public sealed class TimelineNotifier(IHubContext<TimelineHub> hubContext) : ITimelineNotifier
{
    public Task LogEntryCreatedAsync(string sessionId, LogEntryDto entry, CancellationToken ct) =>
        hubContext.Clients.Group(sessionId).SendAsync(TimelineHub.LogEntryCreatedEvent, entry, ct);

    public Task LogEntryUpdatedAsync(string sessionId, LogEntryDto entry, CancellationToken ct) =>
        hubContext.Clients.Group(sessionId).SendAsync(TimelineHub.LogEntryUpdatedEvent, entry, ct);

    public Task LogEntryDeletedAsync(string sessionId, string entryId, CancellationToken ct) =>
        hubContext.Clients.Group(sessionId).SendAsync(TimelineHub.LogEntryDeletedEvent, entryId, ct);

    public Task PlayerJoinedAsync(string sessionId, PlayerInfo player, CancellationToken ct) =>
        hubContext.Clients.Group(sessionId).SendAsync(TimelineHub.PlayerJoinedEvent, player, ct);
}
