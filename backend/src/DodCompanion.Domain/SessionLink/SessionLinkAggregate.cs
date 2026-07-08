using DodCompanion.Domain.Common;

namespace DodCompanion.Domain.SessionLink;

/// <summary>
/// A single-use magic link that authorizes creating a session. Issued to an allowlisted Game Master (SL)
/// email; consuming it (within its TTL, once) creates the room and signs the SL in. Aggregate root.
/// </summary>
public sealed class SessionLinkAggregate : IAggregateRoot
{
    public string Id { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string RoomName { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? ConsumedAt { get; private set; }

    // Parameterless ctor for the RavenDB serializer.
    private SessionLinkAggregate()
    {
    }

    private SessionLinkAggregate(string token, string email, string roomName, DateTimeOffset createdAt, DateTimeOffset expiresAt)
    {
        Token = token;
        Email = email;
        RoomName = roomName;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    /// <summary>Issue a new create-session link for an allowlisted email.</summary>
    public static SessionLinkAggregate Create(string token, string email, string roomName, DateTimeOffset createdAt, DateTimeOffset expiresAt)
    {
        return new SessionLinkAggregate(token, email, roomName, createdAt, expiresAt);
    }

    /// <summary>True while the link is unused and unexpired at <paramref name="now"/>.</summary>
    public bool IsUsable(DateTimeOffset now) => ConsumedAt is null && now < ExpiresAt;

    /// <summary>Mark the link as consumed so it cannot be used again.</summary>
    public void Consume(DateTimeOffset now) => ConsumedAt = now;
}
