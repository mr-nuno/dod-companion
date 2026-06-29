namespace DodCompanion.Application.Common.Interfaces;

/// <summary>
/// Generates cryptographically-random, URL-safe tokens. Used for room join tokens — the unguessable
/// secret the QR code carries. Never derive these from user input or a predictable source.
/// </summary>
public interface ITokenGenerator
{
    /// <summary>A new unguessable, URL-safe join token (≥128 bits of entropy).</summary>
    string NewJoinToken();
}
