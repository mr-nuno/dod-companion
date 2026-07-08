namespace DodCompanion.Application.Common.Configuration;

/// <summary>
/// Session/room-creation policy, bound from the <c>Sessions</c> configuration section and registered as a
/// singleton. Replaces the old static <c>Sessions:CreateKey</c>: room creation is now gated by an allowlist
/// of Game Master (SL) emails that receive a single-use magic link.
/// </summary>
public sealed class SessionOptions
{
    public const string SectionName = "Sessions";

    /// <summary>Emails permitted to request a create-session magic link. Compared case-insensitively.</summary>
    public string[] AllowedDmEmails { get; set; } = [];

    /// <summary>Public base URL of the SPA, used to build the magic link (e.g. <c>https://host/create?token=…</c>).</summary>
    public string AppBaseUrl { get; set; } = string.Empty;

    /// <summary>How long a create-session magic link stays valid.</summary>
    public int MagicLinkTtlMinutes { get; set; } = 15;

    /// <summary>True when <paramref name="email"/> is on the allowlist (case-insensitive, trimmed).</summary>
    public bool IsAllowed(string email) =>
        AllowedDmEmails.Any(allowed => string.Equals(allowed.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase));
}
