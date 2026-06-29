using DodCompanion.Domain.Common;

namespace DodCompanion.Domain.LogEntry;

/// <summary>
/// A single timeline event logged by a player during a session. Aggregate root.
/// References its session by id (<see cref="SessionId"/>), never by object reference.
/// </summary>
public sealed class LogEntryAggregate : IAggregateRoot
{
    public string Id { get; private set; } = string.Empty;
    public string SessionId { get; private set; } = string.Empty;
    public string PlayerName { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset Timestamp { get; private set; }
    public IReadOnlyList<string> Tags { get; private set; } = [];

    // Parameterless ctor for the RavenDB serializer.
    private LogEntryAggregate()
    {
    }

    private LogEntryAggregate(string sessionId, string playerName, string content, DateTimeOffset timestamp, IReadOnlyList<string> tags)
    {
        SessionId = sessionId;
        PlayerName = playerName;
        Content = content;
        Timestamp = timestamp;
        Tags = tags;
    }

    /// <summary>Create a new log entry for a session.</summary>
    public static LogEntryAggregate Create(string sessionId, string playerName, string content, DateTimeOffset timestamp, IReadOnlyList<string>? tags = null)
    {
        return new LogEntryAggregate(sessionId, playerName, content, timestamp, tags ?? []);
    }
}
