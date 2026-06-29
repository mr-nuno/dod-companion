namespace DodCompanion.Application.Common.Dtos;

/// <summary>
/// Shared (Tier 3) timeline entry DTO. Used by the timeline query, the create-log-entry response,
/// and the real-time notifier so all three speak the same shape.
/// </summary>
public sealed record LogEntryDto(
    string Id,
    string SessionId,
    string PlayerName,
    string Content,
    DateTimeOffset Timestamp,
    IReadOnlyList<string> Tags);
