using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DodCompanion.Api.Hubs;

/// <summary>SignalR implementation of the timeline notifier seam — broadcasts to the session's group.</summary>
public sealed class TimelineNotifier(IHubContext<TimelineHub> hubContext) : ITimelineNotifier
{
    public Task LogEntryCreatedAsync(string sessionId, LogEntryDto entry, CancellationToken ct) =>
        hubContext.Clients.Group(sessionId).SendAsync(TimelineHub.LogEntryCreatedEvent, entry, ct);
}
