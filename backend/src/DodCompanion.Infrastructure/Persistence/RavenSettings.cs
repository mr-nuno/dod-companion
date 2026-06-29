namespace DodCompanion.Infrastructure.Persistence;

/// <summary>Bound from the "Raven" configuration section.</summary>
public sealed class RavenSettings
{
    public const string SectionName = "Raven";

    public string[] Urls { get; init; } = [];
    public string DatabaseName { get; init; } = string.Empty;
}
