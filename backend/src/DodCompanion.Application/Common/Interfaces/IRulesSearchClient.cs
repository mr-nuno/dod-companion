using Ardalis.Result;
using DodCompanion.Application.Common.Dtos;

namespace DodCompanion.Application.Common.Interfaces;

/// <summary>
/// Application-owned seam over the external Rules/PDF Search API. The implementation (Infrastructure)
/// attaches the secret Bearer token — handlers never see it.
/// </summary>
public interface IRulesSearchClient
{
    Task<Result<RuleSearchResult>> SearchAsync(string query, CancellationToken ct);
}
