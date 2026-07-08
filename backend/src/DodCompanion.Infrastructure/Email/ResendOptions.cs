namespace DodCompanion.Infrastructure.Email;

/// <summary>
/// Resend (https://resend.com) email settings, bound from the <c>Resend</c> configuration section.
/// The <see cref="ApiKey"/> is a secret (supply via <c>backend/.env</c> / user-secrets). When it is empty
/// the sender runs in log-only mode — useful for local development without hitting Resend.
/// </summary>
public sealed class ResendOptions
{
    public const string SectionName = "Resend";

    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Verified sender address. Resend's <c>onboarding@resend.dev</c> works for testing to your own account.</summary>
    public string FromEmail { get; set; } = "onboarding@resend.dev";

    public string FromName { get; set; } = "DoD Companion";
}
