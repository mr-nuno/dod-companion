namespace DodCompanion.Application.Features.Sessions;

/// <summary>
/// Identifies a created/joined session and the player. Includes the room's <see cref="JoinToken"/> so the
/// client can render the shareable QR code. Used by the endpoints to issue the auth cookie.
/// </summary>
public sealed record SessionResult(string SessionId, string RoomCode, string PlayerName, string JoinToken);
