using DodCompanion.Application.Common.Dtos;

namespace DodCompanion.Application.Common.Interfaces;

/// <summary>
/// Pushes timeline changes to connected players in real time. Implemented in the API layer over SignalR,
/// kept behind this seam so handlers stay free of SignalR and remain unit-testable.
/// </summary>
public interface ITimelineNotifier
{
    Task LogEntryCreatedAsync(string sessionId, LogEntryDto entry, CancellationToken ct);
}
