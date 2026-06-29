namespace DodCompanion.Domain.Session;

/// <summary>Snapshot of a player's character stats at the time they joined the session.</summary>
public sealed record PlayerInfo(
    string Name,
    int Kp,
    int UpptackFara,
    int FinnaDoldaTing);
