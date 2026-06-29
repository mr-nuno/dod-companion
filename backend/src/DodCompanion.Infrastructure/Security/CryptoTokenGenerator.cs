using System.Security.Cryptography;
using DodCompanion.Application.Common.Interfaces;

namespace DodCompanion.Infrastructure.Security;

/// <summary>
/// Produces URL-safe base64 tokens from 16 cryptographically-random bytes (128 bits) — enough entropy
/// that join tokens cannot be guessed or enumerated.
/// </summary>
public sealed class CryptoTokenGenerator : ITokenGenerator
{
    public string NewJoinToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
