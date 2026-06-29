using DodCompanion.Application.Common.Dtos;
using DodCompanion.Domain.LogEntry;

namespace DodCompanion.Application.Features.LogEntries;

/// <summary>Manual mapping for log entries, shared across the create and timeline slices.</summary>
public static class LogEntryMappings
{
    public static LogEntryDto ToDto(this LogEntryAggregate entry) =>
        new(entry.Id, entry.SessionId, entry.PlayerName, entry.Content, entry.Timestamp);
}
