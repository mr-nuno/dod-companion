namespace DodCompanion.Infrastructure.Search;

/// <summary>
/// Bound from the "RulesApi" configuration section. The BFF obtains its bearer token via the
/// OAuth2 client-credentials flow (see <see cref="Auth"/>) — no static token is stored.
/// </summary>
public sealed class RulesApiOptions
{
    public const string SectionName = "RulesApi";

    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>How long to cache identical query results, in seconds.</summary>
    public int CacheSeconds { get; init; } = 60;

    private readonly Dictionary<string, int> _pageModifiers = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, int> PageModifiers
    {
        get => _pageModifiers;
        init
        {
            _pageModifiers.Clear();
            if (value != null)
            {
                foreach (var kvp in value)
                {
                    _pageModifiers[kvp.Key] = kvp.Value;
                }
            }
        }
    }

    public RulesApiAuthOptions Auth { get; init; } = new();
}

/// <summary>
/// OAuth2 client-credentials settings for acquiring the Rules API bearer token.
/// <see cref="ClientSecret"/> is secret — supply it via env var / user-secrets
/// (<c>RulesApi:Auth:ClientSecret</c>), never in committed appsettings.
/// </summary>
public sealed class RulesApiAuthOptions
{
    public string TokenEndpoint { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string GrantType { get; init; } = "client_credentials";
    public string? Scope { get; init; }

    /// <summary>
    /// OAuth2 resource indicator (RFC 8707) — the API the token is requested for. Required by the
    /// Logto tenant for client-credentials tokens; omitting it yields an <c>invalid_target</c> error.
    /// </summary>
    public string? Resource { get; init; }
}
