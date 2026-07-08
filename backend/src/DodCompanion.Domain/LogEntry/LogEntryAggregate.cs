using DodCompanion.Domain.Common;

namespace DodCompanion.Domain.LogEntry;

/// <summary>
/// A single timeline event logged by a player during a session. Aggregate root.
/// References its session by id (<see cref="SessionId"/>), never by object reference.
/// </summary>
public sealed class LogEntryAggregate : IAggregateRoot
{
    /// <summary>
    /// The predefined set of hero banner keys. A random one is assigned to each entry at creation.
    /// The frontend must ship a matching image asset per key (see <c>frontend/public/banners</c>).
    /// </summary>
    public static readonly IReadOnlyList<string> HeroBanners =
        ["ruins", "forest", "dungeon", "tavern", "battlefield", "cave"];

    public string Id { get; private set; } = string.Empty;
    public string SessionId { get; private set; } = string.Empty;
    public string PlayerName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string HeroImage { get; private set; } = string.Empty;
    public DateTimeOffset Timestamp { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public IReadOnlyList<string> Tags { get; private set; } = [];

    // Parameterless ctor for the RavenDB serializer.
    private LogEntryAggregate()
    {
    }

    private LogEntryAggregate(string sessionId, string playerName, string title, string content, string heroImage, DateTimeOffset timestamp, IReadOnlyList<string> tags)
    {
        SessionId = sessionId;
        PlayerName = playerName;
        Title = title;
        Content = content;
        HeroImage = heroImage;
        Timestamp = timestamp;
        Tags = tags;
    }

    /// <summary>Create a new log entry for a session.</summary>
    public static LogEntryAggregate Create(string sessionId, string playerName, string title, string content, string heroImage, DateTimeOffset timestamp, IReadOnlyList<string>? tags = null)
    {
        return new LogEntryAggregate(sessionId, playerName, title, content, heroImage, timestamp, tags ?? []);
    }

    /// <summary>Revise an existing entry's title, content and tags. Hero image and original timestamp are preserved.</summary>
    public void Edit(string title, string content, IReadOnlyList<string> tags, DateTimeOffset updatedAt)
    {
        Title = title;
        Content = content;
        Tags = tags;
        UpdatedAt = updatedAt;
    }
}
