using DodCompanion.Domain.Common;

namespace DodCompanion.Domain.Session;

/// <summary>
/// A play session that players join via a room code. Aggregate root.
/// RavenDB assigns the string <see cref="Id"/> on first store (collection-prefixed, e.g. "sessions/1-A").
/// </summary>
public sealed class SessionAggregate : IAggregateRoot
{
    private readonly HashSet<string> _players = new(StringComparer.OrdinalIgnoreCase);

    // Set-once identity/creation data.
    public string Id { get; private set; } = string.Empty;
    public string RoomCode { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<string> Players => _players;

    // Parameterless ctor for the RavenDB serializer.
    private SessionAggregate()
    {
    }

    private SessionAggregate(string roomCode, DateTimeOffset createdAt)
    {
        RoomCode = roomCode;
        CreatedAt = createdAt;
    }

    /// <summary>Create a new session for the given room code.</summary>
    public static SessionAggregate Create(string roomCode, DateTimeOffset createdAt)
    {
        return new SessionAggregate(roomCode, createdAt);
    }

    /// <summary>Add a player to the session. Idempotent — joining twice is a no-op.</summary>
    public void Join(string playerName)
    {
        _players.Add(playerName);
    }
}
