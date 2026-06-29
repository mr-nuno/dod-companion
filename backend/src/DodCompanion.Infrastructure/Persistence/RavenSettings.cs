namespace DodCompanion.Infrastructure.Persistence;

/// <summary>Bound from the "Raven" configuration section.</summary>
public sealed class RavenSettings
{
    public const string SectionName = "Raven";

    public string[] Urls { get; init; } = [];
    public string DatabaseName { get; init; } = string.Empty;

    /// <summary>Path to an X.509 client certificate (.pfx). Blank for an unsecured store.</summary>
    public string? CertificatePath { get; init; }

    /// <summary>Password for the client certificate; blank when the .pfx has none.</summary>
    public string? CertificatePassword { get; init; }
}
