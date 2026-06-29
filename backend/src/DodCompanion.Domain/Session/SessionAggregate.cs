using DodCompanion.Domain.Common;

namespace DodCompanion.Domain.Session;

/// <summary>
/// A play session. Hosts create it with a friendly room name; players join via the unguessable
/// <see cref="JoinToken"/> carried in the QR code. Aggregate root.
/// RavenDB assigns the string <see cref="Id"/> on first store (collection-prefixed, e.g. "sessions/1-A").
/// </summary>
public sealed class SessionAggregate : IAggregateRoot
{
    private readonly List<PlayerInfo> _players = [];

    // Set-once identity/creation data.
    public string Id { get; private set; } = string.Empty;

    /// <summary>Human-friendly display name (e.g. "DRAGON"). Not a secret — does not grant access.</summary>
    public string RoomCode { get; private set; } = string.Empty;

    /// <summary>Unguessable secret that grants entry. The QR code encodes this; joining requires it.</summary>
    public string JoinToken { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyList<PlayerInfo> Players => _players.AsReadOnly();

    // Parameterless ctor for the RavenDB serializer.
    private SessionAggregate()
    {
    }

    private SessionAggregate(string roomCode, string joinToken, DateTimeOffset createdAt)
    {
        RoomCode = roomCode;
        JoinToken = joinToken;
        CreatedAt = createdAt;
    }

    /// <summary>Create a new session with the given display name and unguessable join token.</summary>
    public static SessionAggregate Create(string roomCode, string joinToken, DateTimeOffset createdAt)
    {
        return new SessionAggregate(roomCode, joinToken, createdAt);
    }

    /// <summary>
    /// Add or update a player in the session. Idempotent — rejoining with the same name updates the player's stats.
    /// </summary>
    public void Join(PlayerInfo player)
    {
        var existing = _players.FindIndex(p =>
            string.Equals(p.Name, player.Name, StringComparison.OrdinalIgnoreCase));

        if (existing >= 0)
            _players[existing] = player;
        else
            _players.Add(player);
    }
}
