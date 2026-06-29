namespace DodCompanion.Application.Common.Interfaces;

/// <summary>
/// The current player's session context, derived from the authentication cookie claims.
/// (Adapted from the standard IUserSession: no real identities — just session + player name.)
/// </summary>
public interface IUserSession
{
    string? SessionId { get; }
    string? PlayerName { get; }
    bool IsAuthenticated { get; }
}
