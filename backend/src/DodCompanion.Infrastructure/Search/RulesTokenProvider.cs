using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DodCompanion.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Serilog;

namespace DodCompanion.Infrastructure.Search;

/// <summary>Supplies a (cached) Rules API access token, acquired via OAuth2 client-credentials.</summary>
public interface IRulesTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken ct);
}

/// <summary>
/// Fetches and caches the bearer token from the OIDC token endpoint, refreshing shortly before expiry.
/// Singleton so the token is shared across requests; a gate prevents a refresh stampede.
/// </summary>
public sealed class RulesTokenProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<RulesApiOptions> options,
    IDateTimeProvider clock) : IRulesTokenProvider
{
    public const string HttpClientName = "RulesAuth";

    private static readonly ILogger Log = Serilog.Log.ForContext<RulesTokenProvider>();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan ExpiryMargin = TimeSpan.FromSeconds(30);

    private readonly SemaphoreSlim _gate = new(1, 1);
    private string? _token;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    public async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        if (IsValid())
        {
            return _token!;
        }

        await _gate.WaitAsync(ct);
        try
        {
            if (IsValid())
            {
                return _token!;
            }

            return await FetchTokenAsync(ct);
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool IsValid() => _token is not null && clock.UtcNow < _expiresAt;

    private async Task<string> FetchTokenAsync(CancellationToken ct)
    {
        var auth = options.Value.Auth;

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = auth.GrantType,
            ["client_id"] = auth.ClientId,
            ["client_secret"] = auth.ClientSecret,
        };

        if (!string.IsNullOrWhiteSpace(auth.Scope))
        {
            form["scope"] = auth.Scope!;
        }

        if (!string.IsNullOrWhiteSpace(auth.Resource))
        {
            form["resource"] = auth.Resource!;
        }

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.PostAsync(auth.TokenEndpoint, new FormUrlEncodedContent(form), ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions, ct)
            ?? throw new InvalidOperationException("Token endpoint returned an empty response.");

        _token = payload.AccessToken;
        _expiresAt = clock.UtcNow.AddSeconds(payload.ExpiresIn) - ExpiryMargin;

        Log.Information("Acquired Rules API token, valid for {ExpiresIn}s", payload.ExpiresIn);
        return _token;
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn,
        [property: JsonPropertyName("token_type")] string? TokenType);
}
