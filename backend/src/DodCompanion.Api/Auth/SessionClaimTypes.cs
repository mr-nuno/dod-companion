namespace DodCompanion.Api.Auth;

/// <summary>Custom claim types carried in the authentication cookie.</summary>
public static class SessionClaimTypes
{
    public const string SessionId = "session_id";
    public const string PlayerName = "player_name";
    public const string RoomCode = "room_code";
    public const string JoinToken = "join_token";
}
