using DodCompanion.Domain.Common;

namespace DodCompanion.Domain.SessionSummary;

/// <summary>A generated markdown summary of a session's timeline. Aggregate root, one per session.</summary>
public sealed class SessionSummaryAggregate : IAggregateRoot
{
    public string Id { get; private set; } = string.Empty;
    public string SessionId { get; private set; } = string.Empty;
    public string RoomCode { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public int EntryCount { get; private set; }
    public DateTimeOffset GeneratedAt { get; private set; }

    // Parameterless ctor for the RavenDB serializer.
    private SessionSummaryAggregate()
    {
    }

    private SessionSummaryAggregate(string sessionId, string roomCode, string content, int entryCount, DateTimeOffset generatedAt)
    {
        SessionId = sessionId;
        RoomCode = roomCode;
        Content = content;
        EntryCount = entryCount;
        GeneratedAt = generatedAt;
    }

    public static SessionSummaryAggregate Create(string sessionId, string roomCode, string content, int entryCount, DateTimeOffset generatedAt) =>
        new(sessionId, roomCode, content, entryCount, generatedAt);

    public void Regenerate(string content, int entryCount, DateTimeOffset generatedAt)
    {
        Content = content;
        EntryCount = entryCount;
        GeneratedAt = generatedAt;
    }
}
