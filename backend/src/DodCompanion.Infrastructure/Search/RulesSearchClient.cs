using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Serilog;

namespace DodCompanion.Infrastructure.Search;

/// <summary>
/// Typed client over the external Rules/PDF Search API. The Bearer token is attached in DI on the
/// underlying <see cref="HttpClient"/> — it never crosses into the Application/Domain layers.
/// </summary>
public sealed class RulesSearchClient(HttpClient httpClient, IOptions<RulesApiOptions> options) : IRulesSearchClient
{
    private static readonly ILogger Log = Serilog.Log.ForContext<RulesSearchClient>();

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<RuleSearchResult>> SearchAsync(string query, CancellationToken ct)
    {
        var requestUri = $"search?query={Uri.EscapeDataString(query)}";

        try
        {
            using var response = await httpClient.GetAsync(requestUri, ct);

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Rules search returned {StatusCode} for query {Query}", (int)response.StatusCode, query);
                return Result.Error($"The rules service returned status {(int)response.StatusCode}.");
            }

            var envelope = await response.Content.ReadFromJsonAsync<SearchApiEnvelope>(JsonOptions, ct);

            if (envelope is not { Success: true, Data: not null })
            {
                return Result.Error(envelope?.Error ?? "The rules service returned an unsuccessful response.");
            }

            return Result.Success(Map(envelope.Data, options.Value));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw; // genuine caller cancellation — let it propagate
        }
        catch (Exception ex)
        {
            // The BFF must never surface a 500 because the upstream is slow/down/circuit-broken
            // (HttpRequestException, Polly TimeoutRejectedException, BrokenCircuitException, …).
            Log.Error(ex, "Rules search failed for query {Query}", query);
            return Result.Error("The rules service is currently unavailable.");
        }
    }

    private static RuleSearchResult Map(SearchApiData data, RulesApiOptions options)
    {
        var hits = (data.Results ?? [])
            .Select(r => {
                var pageModifier = options.PageModifiers.TryGetValue(r.SourceFileName, out var mod) ? mod : 0;
                return new RuleSearchHit(
                    r.SourceFileName,
                    r.PhysicalPageNumber,
                    r.Header,
                    r.Content,
                    r.Tags ?? [],
                    r.SearchScore,
                    pageModifier);
            })
            .ToList();

        return new RuleSearchResult(data.Query, data.ProcessedQuery, data.TotalHits, hits);
    }
}
