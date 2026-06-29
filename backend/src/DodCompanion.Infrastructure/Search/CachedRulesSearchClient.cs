using Ardalis.Result;
using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DodCompanion.Infrastructure.Search;

/// <summary>
/// Caching decorator over <see cref="RulesSearchClient"/>. Handlers depend on <see cref="IRulesSearchClient"/>,
/// so they get caching transparently. Only successful results are cached.
/// </summary>
public sealed class CachedRulesSearchClient(
    RulesSearchClient inner,
    IMemoryCache cache,
    IOptions<RulesApiOptions> options) : IRulesSearchClient
{
    private readonly TimeSpan _ttl = TimeSpan.FromSeconds(Math.Max(0, options.Value.CacheSeconds));

    public async Task<Result<RuleSearchResult>> SearchAsync(string query, CancellationToken ct)
    {
        var key = $"rules-search::{query.Trim().ToLowerInvariant()}";

        if (cache.TryGetValue(key, out RuleSearchResult? cached) && cached is not null)
        {
            return Result.Success(cached);
        }

        var result = await inner.SearchAsync(query, ct);

        if (result.IsSuccess && _ttl > TimeSpan.Zero)
        {
            cache.Set(key, result.Value, _ttl);
        }

        return result;
    }
}
